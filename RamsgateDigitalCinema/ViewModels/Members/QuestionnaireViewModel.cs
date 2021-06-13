using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.ViewModels.Members
{
    public class QuestionnaireViewModel
    {
        public bool IsCollection { get; set; }

        public List<Film> CollectionFilms { get; set; } = new List<Film>();
        public int MemberID { get; set; }

        public int FilmID { get; set; }

        public int? FavouriteFilmID { get; set;}
    }
}
