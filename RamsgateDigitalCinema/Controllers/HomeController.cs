using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Data.Migrations;
using RamsgateDigitalCinema.Extensions;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.ViewModels;
using RamsgateDigitalCinema.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration config, IApiService apiService, UserManager<IdentityUser> userManager, IHttpContextAccessor accessor, ApplicationDbContext context): base(config, apiService, userManager, accessor, context)
        {
        
        }

        public async Task<IActionResult> Index()
        {
            DateTime theTime = await GetLocationTime();
            ViewBag.DateTime = theTime;

            ViewBag.Country = await GetLocation();

            if (User.Identity.IsAuthenticated)
            {
                var member = db.Members.Find(CurrentMember.MemberID);
                member.LastLoggedIn = DateTime.Now;
                db.SaveChanges();

                return RedirectToAction("Index", "Members");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("ShownIntro")))
            {
                ViewBag.ShowIntro = true;
                HttpContext.Session.SetString("ShownIntro", "True");
            }

            LobbyViewModel vm = new LobbyViewModel();

            for (int i = 0; i < 4; i++)
            {
                Screen screen = (Screen)i;

                var film = db.Films.Join(db.FilmDetails, f => f.FilmID, fd => fd.FilmID, (f, fd) => new { Film = f, FilmDetails = fd }).Where(f => f.Film.Showing > theTime && f.FilmDetails.Screen == screen).OrderBy(f => f.Film.FilmID).ThenBy(f => f.Film.Showing).FirstOrDefault();

                if (film != null)
                {
                    vm.Films.Add(new LobbyFilmViewModel()
                    {
                        FilmTitle = film.Film.Title,
                        Rating = film.Film.Rating.GetDescription(),
                        Time = film.Film.Showing.ToString("HH:mm"),
                        Date = film.Film.Showing.ToString("dddd dd MMMM"),
                        PosterUrl = film.FilmDetails.PosterUrl
                    });
                }
                else
                {
                    vm.Films.Add(null);
                }
            }

            return View(vm);
        }

        public IActionResult Programme()
        {
            return View();
        }

        public IActionResult FilmDetails(int id)
        {
            Film film = db.Films.Include(f => f.FilmCategory).Where(f => f.FilmID == id).FirstOrDefault();
            FilmDetails details = db.FilmDetails.Include(fd => fd.StillUrls).Where(fd => fd.FilmID == id).FirstOrDefault();
            List<Film> films = null;

            if (film.FilmCategory.Description == Film.SHORT_COLLECTION)
            {
                FilmCollection col = db.FilmCollections.Where(fc => fc.FilmID == id).FirstOrDefault();

                films = db.Films.Where(f => f.FilmCollectionID == col.FilmCollectionID).ToList();

                foreach (var thisfilm in films)
                {
                    thisfilm.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == thisfilm.FilmID).FirstOrDefault();
                }
            }

            FilmDetailsViewModel vm = new FilmDetailsViewModel() { 
                Film = film,
                FilmDetails = details,
                Films = films
            };

            if (CurrentMember != null)
            {
                var memberFilm = db.MemberFilms.Where(mf => mf.FilmID == id && mf.MemberID == CurrentMember.MemberID).FirstOrDefault();
                ViewBag.Booked = memberFilm != null;
            }
            else
            {
                ViewBag.Booked = false;
            }

            return View(vm);
        }

        public IActionResult Schedule()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult FilmDetailsOld(int id)
        {
            Film film = db.Films.Where(f => f.FilmID == id).FirstOrDefault();
            int shortCollectionID = db.FilmCategories.Where(fc => fc.Description == "Short Collection").Select(x => x.FilmCategoryID).FirstOrDefault();

            if (film.FilmCategoryID == shortCollectionID)
            {
                return RedirectToAction("FilmCollectionDetails", new { id = id });
            }

            film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == id).FirstOrDefault();

            return View(film);
        }

        public IActionResult FilmCollectionDetails(int id)
        {
            FilmCollection col = db.FilmCollections.Include(fc => fc.Films).Where(fc => fc.FilmID == id).FirstOrDefault();

            col.Film = db.Films.Find(id);

            foreach (var film in col.Films)
            {
                film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();
            }

            return View(col);
        }

        public IActionResult Films()
        {
            FilmsViewModel vm = new FilmsViewModel();

            var categories = db.FilmCategories.OrderBy(fc => fc.Description).Where(fc => fc.IsViewable && fc.FilmCategoryID != 8).ToList().Select(fc => new FilmCategoryViewModel()
            {
                Category = fc,
                Films = db.Films.Where(f => fc.FilmCategoryID == f.FilmCategoryID).ToList()
            }).ToList();

            foreach (var cat in categories)
            {
                foreach (var film in cat.Films)
                {
                    film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();
                }
            }

            vm.Categories = categories;

            ViewBag.LoggedIn = CurrentMember != null;

            return View(vm);
        }

        public IActionResult Collections()
        {
            CollectionsViewModel vm = new CollectionsViewModel();

            var collections = db.FilmCollections.OrderBy(fc => fc.Name).ToList().Select(fc => new CollectionViewModel()
            {
                Collection = fc,
                Films = db.Films.Where(f => fc.FilmCollectionID == f.FilmCollectionID && f.Title != "Collection").ToList()
            }).ToList();

            foreach (var col in collections)
            {
                foreach (var film in col.Films)
                {
                    film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();
                }
            }

            vm.Collections = collections;

            ViewBag.LoggedIn = CurrentMember != null;

            return View(vm);
        }
    }
}
