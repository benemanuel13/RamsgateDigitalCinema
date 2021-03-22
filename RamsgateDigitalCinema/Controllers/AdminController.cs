using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.Models.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.IO;

namespace RamsgateDigitalCinema.Controllers
{
    public enum FileType
    {
        Trailer,
        Poster,
        Still,
        Director
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext db;

        public AdminController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet("GetFilmCategories")]
        public List<FilmCategory> GetFilmCategories()
        {
            var cats = db.FilmCategories.ToList();

            return cats;
        }

        [HttpPost("PostFilmCategory")]
        public FilmCategory PostFilmCategory(FilmCategory cat)
        {
            var original = db.FilmCategories.Find(cat.FilmCategoryID);

            if (original == null)
            {
                db.FilmCategories.Add(cat);
                db.SaveChanges();

                return cat;
            }

            original.Description = cat.Description;
            db.SaveChanges();

            return original;
        }

        [HttpGet("GetFilmCollections")]
        public List<FilmCollection> GetFilmCollections()
        {
            var cols = db.FilmCollections.ToList();

            foreach (var collection in cols)
            {
                collection.Film = db.Films.Find(collection.FilmID);
            }

            return cols;
        }

        [HttpPost("PostFilmCollection")]
        public FilmCollection PostFilmCollection(FilmCollection col)
        {
            var original = db.FilmCollections.Find(col.FilmCollectionID);

            if (original == null)
            {
                db.FilmCollections.Add(col);
                db.SaveChanges();

                return col;
            }

            original.Name = col.Name;
            db.SaveChanges();

            return original;
        }

        [HttpGet("GetFilms")]
        public List<Film> GetFilms(int id)
        {
            List<Film> films = null;

            if (id == 0)
            {
                films = db.Films.Where(f => f.FilmCollectionID != 0).ToList();
            }
            else
            {
                films = db.Films.Where(f => f.FilmCollectionID == id).ToList();
            }

            return films;
        }

        [HttpPost("PostFilm")]
        public Film PostFilm(Film film)
        {
            var original = db.Films.Find(film.FilmID);

            if (original == null)
            {
                db.Films.Add(film);
                db.SaveChanges();

                return film;
            }

            original.Title = film.Title;
            original.Rating = film.Rating;
            original.Showing = film.Showing;
            db.SaveChanges();

            return original;
        }

        [HttpGet("GetRegions")]
        public List<Region> GetRegions()
        {
            var regions = db.Regions.ToList();

            return regions;
        }

        [HttpGet("GetSubRegions")]
        public List<SubRegion> GetSubRegions(int id)
        {
            var subRegions = db.SubRegions.Where(sr => sr.RegionID == id).ToList();

            return subRegions;
        }

        [HttpGet("GetCountries")]
        public List<Country> GetCountries(int id)
        {
            var countries = db.Countries.Where(c => c.SubRegionID == id).ToList();

            return countries;
        }

        [HttpGet("GetFilmDetails")]
        public FilmDetails GetFilmDetails(int id)
        {
            Film film = db.Films.Find(id);
            FilmDetails details = db.FilmDetails.Include(fd => fd.Film).Include(fd => fd.StillUrls).Where(fd => fd.FilmID == id).FirstOrDefault();

            if (details == null)
            {
                details = new FilmDetails()
                {
                    FilmID = id,
                    Film = film,
                    StillUrls = new List<StillUrl>()
                };

                db.FilmDetails.Add(details);
                db.SaveChanges();
            }

            return details;
        }

        [HttpPost("PostFile")]
        [RequestSizeLimit(41201000000)]
        public string PostFile(int id, FileType fileType)
        {
            string message = "OK";

            if (!Request.Form.Files.Any())
            {
                return "Failure";
            }

            string subFolder = "";

            try
            {
                if (fileType == FileType.Trailer)
                {
                    subFolder = "Trailers";
                }
                else if (fileType == FileType.Poster)
                {
                    subFolder = "Posters";
                }
                else if (fileType == FileType.Still)
                {
                    subFolder = "Stills";
                }
                else if (fileType == FileType.Director)
                {
                    subFolder = "Directors";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            string myFileName = "";
            string folder = AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\" + subFolder + "\\";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            try
            {
                IFormFile myFile = Request.Form.Files.First();

                //myFileName = myFile.Name.Replace(" ", "");
                myFileName = Guid.NewGuid().ToString() + Path.GetExtension(myFile.Name);

                FileStream stream = new FileStream(folder + myFileName, FileMode.Create);

                myFile.CopyTo(stream);

                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            Film film = null;
            FilmDetails details = null;

            try
            {
                film = db.Films.Find(id);
                details = db.FilmDetails.Include(fd => fd.Film).Include(fd => fd.StillUrls).Where(fd => fd.FilmID == id).FirstOrDefault();

                if (details == null)
                {
                    details = new FilmDetails()
                    {
                        FilmID = id,
                        Film = film,
                        StillUrls = new List<StillUrl>()
                    };

                    db.FilmDetails.Add(details);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            try
            {
                if (fileType == FileType.Trailer)
                {
                    details.TrailerUrl = "/" + subFolder + "/" + myFileName;
                }
                else if (fileType == FileType.Poster)
                {
                    details.PosterUrl = "/" + subFolder + "/" + myFileName;
                }
                else if (fileType == FileType.Still)
                {
                    details.StillUrls.Add(new StillUrl() { Url = "/" + subFolder + "/" + myFileName });
                }
                else if (fileType == FileType.Director)
                {
                    details.DirectorPicUrl = "/" + subFolder + "/" + myFileName;
                }

                message = "OK" + myFileName;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }


        [HttpPost("UpdateFilmDetails")]
        public string UpdateFilmDetails([FromBody] FilmDetails filmDetails)
        {
            FilmDetails original = db.FilmDetails.Find(filmDetails.FilmDetailsID);

            db.Entry(original).CurrentValues.SetValues(filmDetails);
            db.SaveChanges();

            return filmDetails.FilmDetailsID.ToString();
        }
    }
}
