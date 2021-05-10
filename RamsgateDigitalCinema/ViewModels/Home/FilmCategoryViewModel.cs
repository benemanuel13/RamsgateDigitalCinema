using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Home
{
    public class FilmCategoryViewModel
    {
        public FilmCategory Category { get; set; }

        public List<Film> Films { get; set; }
    }
}
