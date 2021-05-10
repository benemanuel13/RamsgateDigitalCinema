using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Home
{
    public class CollectionViewModel
    {
        public FilmCollection Collection { get; set; }
        public List<Film> Films { get; set; }
    }
}
