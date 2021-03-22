using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers
{
    [Authorize]
    public class MembersController : BaseController
    {
        public MembersController(IConfiguration config, IApiService apiService, UserManager<IdentityUser> userManager, IHttpContextAccessor accessor, ApplicationDbContext context) : base(config, apiService, userManager, accessor, context)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Screen(Screen screen)
        {
            DateTime currentTime = await GetLocationTime();

            var nextFilm = db.Films.Include(f => f.FilmCollection).Include(f => f.FilmCategory).Join(db.FilmDetails, f => f.FilmID, fd => fd.FilmID, (f, fd) => new { Film = f, Details = fd }).Where(
                    x => x.Details.Screen == screen && x.Film.Showing > currentTime && x.Film.FilmCategory.IsViewable).OrderBy(x => x.Film.Showing).FirstOrDefault();

            return View();
        }
    }
}
