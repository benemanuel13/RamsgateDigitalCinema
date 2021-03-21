using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Localisation
{
    public class Country
    {
        public int CountryID { get; set; }

        public int SubRegionID { get; set; }
        [ForeignKey("SubRegionID")]
        public SubRegion SubRegion { get; set; }

        public string Name { get; set; }
        public string CountryCode { get; set; }
    }
}
