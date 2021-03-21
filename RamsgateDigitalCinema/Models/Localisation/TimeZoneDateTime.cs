using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Localisation
{
    public class TimeZoneDateTime
    {
        public string date_time_ymd { get; set; }

        public string time { get; set; }

        [JsonIgnore]
        public DateTime DateTime
        {
            get {
                return DateTime.Parse(date_time_ymd.Substring(0, 10) + "T" + time);
            }
        }
    }
}
