using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models;
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
    }
}
