using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

using RamsgateDigitalCinema.Models.Entities;

namespace RamsgateDigitalCinema.Models.Entities
{
    public enum Screen
    {
        [Description("Screen One")]
        Screen1,
        [Description("Screen Two")]
        Screen2,
        [Description("Screen Three")]
        Screen3,
        [Description("Screen Four")]
        Screen4
    }

    public class FilmDetails
    {
        public int FilmDetailsID { get; set; }

        public int FilmID { get; set; }

        [ForeignKey("FilmID")]
        public Film Film { get; set; }

        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public string DirectorPicUrl { get; set; }
        public string DirectorPicUrl2 { get; set; }
        public virtual List<StillUrl> StillUrls { get; set; }

        public string DirectorIntro { get; set; }
        public string Cast { get; set; }

        public string CountryOfOrigin { get; set; }
        public int FilmLength { get; set; }
        public string Synopsis { get; set; }

        public string DirectorOrigin { get; set; }
        public string DirectorBio { get; set; }
        public bool DirectorsFirstFilm { get; set; }

        public Screen Screen { get; set; }
    }
}
