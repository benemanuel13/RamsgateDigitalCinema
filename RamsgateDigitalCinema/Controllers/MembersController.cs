using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
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
    }
}
