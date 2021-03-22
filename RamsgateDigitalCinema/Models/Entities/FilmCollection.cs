using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public class FilmCollection
    {
        public int FilmCollectionID { get; set; }

        public int FilmID { get; set; }

        [NotMapped]
        public Film Film { get; set; }

        public string Name { get; set; }

        public virtual List<Film> Films { get; set; }
    }
}
