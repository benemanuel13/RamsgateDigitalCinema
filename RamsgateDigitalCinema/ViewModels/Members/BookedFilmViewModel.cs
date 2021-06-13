using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Members
{
    public class BookedFilmViewModel
    {
        public int FilmID { get; set; }
        public string Title { get; set; }
        public DateTime Showing { get; set; }

        public bool IsExtra { get; set; } = false;
    }
}
