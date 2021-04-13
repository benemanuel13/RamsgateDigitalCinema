using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels
{
    public class FilmDetailsViewModel
    {
        public Film Film { get; set; }

        public FilmDetails FilmDetails { get; set; }

        public List<Film> Films { get; set; } = null;
    }
}
