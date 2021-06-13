using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Home
{
    public class AwardsExtrasViewModel
    {
        public List<Film> Saturday { get; set; }
        public List<Film> Sunday { get; set; }
    }
}
