using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models
{
    public class Questionnaire
    {
        public int QuestionnaireID { get; set; }

        public int FilmID { get; set; }
        public int MemberID { get; set; }

        public bool IsCollection { get; set; }

        public List<Film> Films { get; set; }

        public int? FavouriteFilmID { get; set; }

        public string FilmTitle { get; set; }

        //Questions

        public int Like { get; set; }
        public int LikeFestival { get; set; }

        public int Booked { get; set; } = 1;
        public bool BookedEvent { get; set; }

        public string Event { get; set; }

        public int Overall { get; set; }
        public int TheFilms { get; set; }
        public int TheEvents { get; set; }
        public int Networking { get; set; }

        public string DoBetter { get; set; }

        public bool ComeNext { get; set; }

        public int WhereFrom { get; set; }

        public bool Industry { get; set; }

        public int Gender { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
    }
}
