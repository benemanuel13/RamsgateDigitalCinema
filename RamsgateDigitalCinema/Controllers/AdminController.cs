using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.Models.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers
{
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
                films = db.Films.Where(f => !f.FilmCollectionID.HasValue).ToList();
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
    }
}
