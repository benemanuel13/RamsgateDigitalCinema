using RamsgateDigitalCinema.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public class FilmCollection
    {
        ApplicationDbContext db;

        public FilmCollection(ApplicationDbContext context)
        {
            db = context;
        }

        public int FilmCollectionID { get; set; }

        public int FilmID { get; set; }

        [NotMapped]
        public Film Film { get; set; }

        public string Name { get; set; }

        public virtual List<Film> Films { get; set; }

        public int SortOrder { get; set; }

        //[NotMapped]
        //public List<Film> FilteredFilms
        //{
        //    get {
        //        FilmCategory cat = db.FilmCategories.Where(fc => fc.Description != Film.SHORT_COLLECTION).FirstOrDefault();

        //        return Films.Where(f => f.FilmCategoryID != cat.FilmCategoryID).ToList();
        //    }
        //}
    }
}
