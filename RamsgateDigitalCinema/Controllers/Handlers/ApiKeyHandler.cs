using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using Microsoft.Extensions.Configuration;

namespace RamsgateDigitalCinema.Controllers.Handlers
{
    public class ApiKeyMiddleware
    {
        RequestDelegate next = null;
        public string AdminKey { get; private set; } = "not set";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext db, IApiService apiService, IConfiguration config)
        {
            AdminKey = config.GetSection("ClientApiKey").Value;

            string key = context.Request.Headers["Authorization"];

            if (key != AdminKey && context.Request.Path.StartsWithSegments("/api/Admin"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            else if (context.Request.Path.StartsWithSegments("/api/App"))
            {
                var thisUser = db.Members.Where(m => m.ApiKey == key).FirstOrDefault();

                if (thisUser == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                else
                {
                    context.Items.Add("MemberID", thisUser.MemberID);
                }
            }
            else if (context.Request.Path.StartsWithSegments("/AppWeb"))
            {
                var thisUser = db.Members.Where(m => m.ApiKey == key).FirstOrDefault();

                if (thisUser == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                else
                {
                    context.Items.Add("MemberID", thisUser.MemberID);
                }
            }
                
            await next.Invoke(context);
        }
    }
}
