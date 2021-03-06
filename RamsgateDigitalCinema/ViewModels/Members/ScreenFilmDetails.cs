using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Members
{
    public class ScreenFilmDetails
    {
        public int Screen { get; set; }

        public string Title { get; set; }
        public Film Film { get; set; }
        public FilmDetails FilmDetails { get; set;}

        public bool Blocked { get; set; } = false;

        public bool Booked { get; set; }
    }
}
