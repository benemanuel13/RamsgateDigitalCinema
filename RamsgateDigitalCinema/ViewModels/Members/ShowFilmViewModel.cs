using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Members
{
    public class ShowFilmViewModel
    {
        public int FilmID { get; set; }
        public int MemberID { get; set; }
        public Film Film { get; set; }

        public string Token { get; set; }

        public string Source { get; set; }
    }
}
