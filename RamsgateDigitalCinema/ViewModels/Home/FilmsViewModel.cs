using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Home
{
    public class FilmsViewModel
    {
        public List<FilmCategoryViewModel> Categories { get; set; }
    }
}
