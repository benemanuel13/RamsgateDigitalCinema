using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers.Handlers
{
    public class RequestData
    {
        public string IPAddress { get; set; }
        public string Url { get; set; }
        public DateTime LastRequested { get; set; }
    }
}
