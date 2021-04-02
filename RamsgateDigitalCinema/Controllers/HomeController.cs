using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models;
using RamsgateDigitalCinema.Models.Entities;
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
            ViewBag.DateTime = await GetLocationTime();

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
            
            return View();
        }

        public IActionResult Programme()
        {
            return View();
        }

        public IActionResult Films()
        {
            return View();
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

        public IActionResult FilmDetails(int id)
        {
            Film film = db.Films.Where(f => f.FilmID == id).FirstOrDefault();
            int shortCollectionID = db.FilmCategories.Where(fc => fc.Description == "Short Collection").Select(x => x.FilmCategoryID).FirstOrDefault();

            if (film.FilmCategoryID == shortCollectionID)
            {
                return RedirectToAction("FilmCollectionDetauls", new { id = id });
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
    }
}
