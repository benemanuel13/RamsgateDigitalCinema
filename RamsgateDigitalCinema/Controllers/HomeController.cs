using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        public HomeController(IEmailSender emailSender, IConfiguration config, IApiService apiService, UserManager<IdentityUser> userManager, IHttpContextAccessor accessor, ApplicationDbContext context): base(emailSender, config, apiService, userManager, accessor, context)
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

                //return RedirectToAction("Index", "Members");
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

                var film = db.Films.Join(db.FilmDetails.Include(fd => fd.StillUrls), f => f.FilmID, fd => fd.FilmID, (f, fd) => new { Film = f, FilmDetails = fd }).Where(f => f.Film.Showing > theTime && f.FilmDetails.Screen == screen && f.Film.FilmCollectionID == null).OrderBy(f => f.Film.Showing).ThenBy(f => f.Film.FilmID).FirstOrDefault();

                if (film != null)
                {
                    var title = film.Film.Title == "Collection" ? db.FilmCollections.Where(fc => fc.FilmID == film.Film.FilmID).First().Name : film.Film.Title;

                    vm.Films.Add(new LobbyFilmViewModel()
                    {
                        FilmTitle = title,
                        Rating = film.Film.Rating.GetDescription(),
                        Time = film.Film.Showing.ToString("HH:mm"),
                        Date = film.Film.Showing.ToString("dddd dd MMMM"),
                        PosterUrl = film.FilmDetails.PosterUrl,
                        FilmDetails = film.FilmDetails
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

        public async Task<ActionResult> AwardsFilms()
        {
            bool loggedIn = CurrentMember != null;
            ViewBag.LoggedIn = loggedIn;

            if (loggedIn)
            {
                var bookedFilms = db.MemberFilms.Where(mf => mf.MemberID == CurrentMember.MemberID).ToList();
                ViewBag.BookedFilms = bookedFilms;
            }

            ViewBag.CurrentTime = await GetLocationTime();

            CollectionsViewModel vm = new CollectionsViewModel();

            var collections = db.FilmCollections.ToList();

            foreach (var col in collections)
            {
                Film film = db.Films.Find(col.FilmID);

                col.Film = film;
            }

            var colles = collections.Where(c => c.Film.Showing > DateTime.Parse("2021-06-10")).OrderBy(fc => fc.Name).OrderBy(fc => fc.SortOrder).ToList().Select(fc => new CollectionViewModel()
            {
                Collection = fc,
                Films = db.Films.Where(f => fc.FilmCollectionID == f.FilmCollectionID && f.Title != "Collection").ToList()
            }).ToList();

            foreach (var col in colles)
            {
                foreach (var film in col.Films)
                {
                    film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();
                }
            }

            vm.Collections = colles;

            return View(vm);
        }

        public async Task<IActionResult> Films()
        {
            FilmsViewModel vm = new FilmsViewModel();

            var categories = db.FilmCategories.OrderBy(fc => fc.Description).Where(fc => fc.IsViewable && fc.FilmCategoryID != 8).OrderBy(fc => fc.OrderPosition).ToList().Select(fc => new FilmCategoryViewModel()
            {
                Category = fc,
                Films = db.Films.Where(f => fc.FilmCategoryID == f.FilmCategoryID && f.FilmCollectionID == null).OrderBy(f => f.Showing).ToList()
            }).ToList();

            foreach (var cat in categories)
            {
                foreach (var film in cat.Films)
                {
                    film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();
                }
            }

            vm.Categories = categories;

            bool loggedIn = CurrentMember != null;
            ViewBag.LoggedIn = loggedIn;

            if (loggedIn)
            {
                var bookedFilms = db.MemberFilms.Where(mf => mf.MemberID == CurrentMember.MemberID).ToList();
                ViewBag.BookedFilms = bookedFilms;
            }

            ViewBag.CurrentTime = await GetLocationTime();

            return View(vm);
        }

        public async Task<IActionResult> Collections()
        {
            CollectionsViewModel vm = new CollectionsViewModel();

            var collections = db.FilmCollections.OrderBy(fc => fc.Name).OrderBy(fc => fc.SortOrder).ToList().Select(fc => new CollectionViewModel()
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

            bool loggedIn = CurrentMember != null;
            ViewBag.LoggedIn = loggedIn;

            if (loggedIn)
            {
                var bookedFilms = db.MemberFilms.Where(mf => mf.MemberID == CurrentMember.MemberID).ToList();
                ViewBag.BookedFilms = bookedFilms;
            }

            ViewBag.CurrentTime = await GetLocationTime();

            ViewBag.Database = db;

            return View(vm);
        }

        public ActionResult Events()
        {
            return RedirectToAction("Extras");
        }

        public async Task<ActionResult> Extras()
        {
            bool loggedIn = CurrentMember != null;
            ViewBag.LoggedIn = loggedIn;

            if (loggedIn)
            {
                var bookedFilms = db.MemberFilms.Where(mf => mf.MemberID == CurrentMember.MemberID).ToList();
                ViewBag.BookedFilms = bookedFilms;
            }

            ExtrasViewModel vm = new ExtrasViewModel() { 
                WhiteTiger = db.Films.Find(240),
                Opening = db.Films.Find(243),
                CODumentary = db.Films.Find(241),
                CODumentaryConverse = db.Films.Find(244),
                Agent = db.Films.Find(245),
                VFX = db.Films.Find(242),
                Network = db.Films.Find(246),
                Kajaki = db.Films.Find(247),
                African = db.Films.Find(248),
                Cycling = db.Films.Find(186),
                Tonton = db.Films.Find(249),
                Awards = db.Films.Find(250)
            };

            vm.WhiteTiger.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == 240).FirstOrDefault();
            vm.Kajaki.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == 247).FirstOrDefault();

            ViewBag.CurrentTime = await GetLocationTime();

            return View(vm);
        }

        public async  Task<ActionResult> AwardsExtras()
        {
            AwardsExtrasViewModel vm = new AwardsExtrasViewModel() {
                Saturday = db.Films.Where(f => f.FilmCategoryID == 26 && f.Showing > DateTime.Parse("2021-06-19 00:00") && f.Showing < DateTime.Parse("2021-06-19 23:59")).ToList(),
                Sunday = db.Films.Where(f => f.FilmCategoryID == 26 && f.Showing > DateTime.Parse("2021-06-20 00:00") && f.Showing < DateTime.Parse("2021-06-20 23:59")).ToList()
            };

            bool loggedIn = CurrentMember != null;
            ViewBag.LoggedIn = loggedIn;

            if (loggedIn)
            {
                var bookedFilms = db.MemberFilms.Where(mf => mf.MemberID == CurrentMember.MemberID).ToList();
                ViewBag.BookedFilms = bookedFilms;
            }

            ViewBag.CurrentTime = await GetLocationTime();

            return View(vm);
        }

        public ActionResult Questionnaire(int filmID, int memberID)
        {
            MemberFilm mf = db.MemberFilms.Where(x => x.FilmID == filmID && x.MemberID == memberID).FirstOrDefault();
            
            Film film = db.Films.Find(filmID);
            Member member = db.Members.Find(memberID);

            var aspUser = db.Users.Find(member.ASPID);

            bool isCollection = false;

            if (film.Title == "Collection")
            {
                isCollection = true;
            }

            Questionnaire q = new Questionnaire() { 
                FilmID = filmID,
                MemberID = memberID,
                IsCollection = isCollection,
                FilmTitle = film.Title,
                Email = aspUser.Email
            };

            if (isCollection)
            {
                var coll = db.FilmCollections.Where(c => c.FilmID == filmID).First();
                var films = db.Films.Where(f => f.FilmCollectionID == coll.FilmCollectionID && f.FilmID != filmID).ToList();

                q.FilmTitle = coll.Name;
                q.Films = films;
            }

            List<ScaleViewModel> scales = new List<ScaleViewModel>();

            for (int i = 1; i < 11; i++)
            {
                scales.Add(new ScaleViewModel() { 
                    Value = i,
                    Text = i.ToString()
                });
            }

            ViewBag.Scales = scales;

            List<ScaleViewModel> from = new List<ScaleViewModel>() {
                new ScaleViewModel(){ Value = 0, Text = "Ramsgate" },
                new ScaleViewModel(){ Value = 1, Text = "Kent" },
                new ScaleViewModel(){ Value = 2, Text = "UK" },
                new ScaleViewModel(){ Value = 3, Text = "Europe" },
                new ScaleViewModel(){ Value = 4, Text = "Americas" },
                new ScaleViewModel(){ Value = 5, Text = "Asia" },
                new ScaleViewModel(){ Value = 6, Text = "Africa" },
                new ScaleViewModel(){ Value = 7, Text = "Oceania" }
            };

            ViewBag.Locations = from;

            List<ScaleViewModel> gender = new List<ScaleViewModel>() {
                new ScaleViewModel(){ Value = 0, Text = "Male" },
                new ScaleViewModel(){ Value = 1, Text = "Female" },
                new ScaleViewModel(){ Value = 2, Text = "Transgender" },
                new ScaleViewModel(){ Value = 3, Text = "Non-binary" },
                new ScaleViewModel(){ Value = 4, Text = "Prefer no answer" }
            };

            ViewBag.Gender = gender;

            List<ScaleViewModel> ages = new List<ScaleViewModel>() {
                new ScaleViewModel(){ Value = 0, Text = "8 to 14" },
                new ScaleViewModel(){ Value = 1, Text = "15 to 18" },
                new ScaleViewModel(){ Value = 2, Text = "19 to 25" },
                new ScaleViewModel(){ Value = 3, Text = "26 to 45" },
                new ScaleViewModel(){ Value = 4, Text = "46 to 65" },
                new ScaleViewModel(){ Value = 4, Text = "Over 65" }
            };

            ViewBag.Ages = ages;

            return View(q);
        }

        [HttpPost]
        public ActionResult Questionnaire(Questionnaire q)
        {
            db.Questionnaires.Add(q);
            db.SaveChanges();

            return RedirectToAction("ThankYou");
        }

        public ActionResult ThankYou()
        {
            
            return View();
        }

        public ActionResult Unsubscribe(string code)
        {
            var member = db.Members.Where(m => m.UnsubscribeCode == code).FirstOrDefault();

            if (member != null)
            {
                member.DontEmail = true;
                db.SaveChanges();
            }

            return View();
        }

        public async Task<ActionResult> SendQuestions()
        {
            var films = db.MemberFilms.Where(mf => mf.Token != null).ToList();

            int count = 0;

            foreach (var film in films)
            {
                var member = db.Members.Find(film.MemberID);

                if (member.DontEmail)
                {
                    continue;
                }

                count++;

                var thefilm = db.Films.Find(film.FilmID);
                var email = db.Users.Find(member.ASPID).Email;

                bool isCollection = false;

                if (thefilm.Title == "Collection")
                {
                    isCollection = true;
                }

                string title = thefilm.Title;

                if (isCollection)
                {
                    var coll = db.FilmCollections.Where(c => c.FilmID == film.FilmID).First();

                    title = coll.Name;
                }

                string message = "<p>QUESTIONAIRRE</p>";
                message += "<p>Would you kindly fill in a questionairre about " + title + "</p>";
                message += "<p>Use this <a href='https://www.ramsgatedigitalcinema.co.uk/Home/Questionnaire?filmID=" + film.FilmID + "&memberID=" + film.MemberID + "'>LINK</a> to go to questionnaire</p>";
                message += "<p>Use this <a href='https://www.ramsgatedigitalcinema.co.uk/Home/Unsubscribe?code=" + member.UnsubscribeCode + "'>LINK</a> to unscribe from out emails.</p>";

                await _emailSender.SendEmailAsync(email, "Questionairre", message);
            }

            return Content("OK: " + count.ToString());
        }
    }
}
