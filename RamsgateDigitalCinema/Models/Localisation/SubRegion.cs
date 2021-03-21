using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Localisation
{
    public class SubRegion
    {
        public int SubRegionID { get; set; }

        public int RegionID { get; set; }
        [ForeignKey("RegionID")]
        public Region Region { get; set; }

        public string Name { get; set; }

        public virtual List<Country> Countries { get; set; }
    }
}
