using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public class FilmCategory
    {
        public int FilmCategoryID { get; set; }

        public string Description { get; set; }

        public bool IsViewable { get; set; }

        public int OrderPosition { get; set; }
}
}
