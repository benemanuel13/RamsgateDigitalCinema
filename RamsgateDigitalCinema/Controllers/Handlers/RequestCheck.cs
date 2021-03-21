using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers.Handlers
{
    public class RequestCheck
    {
        static readonly object _object = new object();

        RequestDelegate next = null;

        protected static List<RequestData> requests = new List<RequestData>();

        public RequestCheck(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string ip = context.Connection.RemoteIpAddress.ToString();
            string url = context.Request.GetDisplayUrl();
            DateTime date = DateTime.Now;

            lock (_object)
            {
                if (url.Contains("ViewBook") || url.Contains("FilmDataList") || url.Contains("Unsubscribe"))
                {
                    //Do Nothing
                }
                else
                {
                    url = context.Request.GetDisplayUrl() + context.Request.QueryString;

                    var reqs = requests.Where(r => r.IPAddress == ip && r.Url == url && date.Subtract(r.LastRequested).TotalSeconds < 5).Count();

                    if (reqs > 1)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    requests.Add(new RequestData()
                    {
                        IPAddress = ip,
                        Url = url,
                        LastRequested = date
                    });

                    requests.RemoveAll(r => date.Subtract(r.LastRequested).TotalSeconds > 8);
                }
            }

            await next.Invoke(context);
        }
    }
}
