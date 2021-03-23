using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public enum Rating
    {
        [Description("Universal")]
        Universal,
        [Description("Parental Guidence")]
        ParentalGuidence
    }

    public class Film
    {
        [NotMapped]
        public static string SHORT_COLLECTION = "Short Collection";

        public int FilmID { get; set; }

        public bool Uploaded { get; set; }
        public bool AssetCreated { get; set; }

        public string Title { get; set; }

        public DateTime Showing { get; set; }

        [NotMapped]
        public FilmDetails FilmDetails { get; set; }


        public int? FilmCollectionID { get; set; }
        [ForeignKey("FilmCollectionID")]
        public FilmCollection FilmCollection { get; set; }

        public Rating Rating { get; set; }

        public int FilmCategoryID { get; set; }
        [ForeignKey("FilmCategoryID")]
        public FilmCategory FilmCategory { get; set; }

        public string RemoteFileName { get; set; }
    }
}
