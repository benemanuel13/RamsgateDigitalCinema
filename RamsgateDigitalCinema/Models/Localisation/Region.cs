using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Localisation
{
    public class Region
    {
        public int RegionID { get; set; }

        public string Name { get; set; }

        public virtual List<SubRegion> SubRegions { get; set; }
    }
}
