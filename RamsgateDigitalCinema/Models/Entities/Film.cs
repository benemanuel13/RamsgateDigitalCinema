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
        [Description("U")]
        Universal,
        [Description("PG")]
        ParentalGuidence,
        [Description("12A")]
        X12A,
        [Description("15")]
        X15,
        [Description("18")]
        X18
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

        public string Director { get; set; }


        public Rating Rating { get; set; }

        public int FilmCategoryID { get; set; }
        [ForeignKey("FilmCategoryID")]
        public FilmCategory FilmCategory { get; set; }

        public string RemoteFileName { get; set; }

        public string AssetName { get; set; }

        public string Source { get; set; }

        public int Booked { get; set; }
        public int Watched { get; set; }
    }
}
