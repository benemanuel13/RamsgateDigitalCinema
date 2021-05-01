using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Home
{
    public class LobbyViewModel
    {
        public List<LobbyFilmViewModel> Films { get; } = new List<LobbyFilmViewModel>();
    }

    public class LobbyFilmViewModel
    {
        public string Time { get; set; }
        public string Date { get; set; }
        public string FilmTitle { get; set; }
        public string Rating { get; set; }
        public string PosterUrl { get; set; }
    }
}
