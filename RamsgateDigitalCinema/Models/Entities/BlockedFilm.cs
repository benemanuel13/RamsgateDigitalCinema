using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public class BlockedFilm
    {
        public int BlockedFilmID { get; set; }

        public int FilmID { get; set; }
        public int CountryID { get; set; }
    }
}
