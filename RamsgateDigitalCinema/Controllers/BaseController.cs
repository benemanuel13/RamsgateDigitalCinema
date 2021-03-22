using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.Models.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<IdentityUser> _userManager;
        protected readonly IHttpContextAccessor _accessor;
        protected readonly ApplicationDbContext db;
        protected readonly IApiService _apiService;
        protected readonly IConfiguration _config;

        public bool UserAuthenticated => User.Identity.IsAuthenticated;

        public string LoggedInUserASPID { get; private set; }

        public IdentityUser ASPUser => _userManager.GetUserAsync(HttpContext.User).Result;

        public Member CurrentMember { get; private set; } = null;

        public BaseController(IConfiguration config, IApiService apiService, UserManager<IdentityUser> userManager, IHttpContextAccessor accessor, ApplicationDbContext context)
        {
            _userManager = userManager;
            _accessor = accessor;
            _apiService = apiService;
            _config = config;

            db = context;

            try
            {
                LoggedInUserASPID = _userManager.GetUserAsync(accessor.HttpContext.User).Result?.Id;
                CurrentMember = db.Members.Where(m => m.ASPID == LoggedInUserASPID).FirstOrDefault();
            }
            catch
            {
                
            }
        }

        public async Task<DateTime> GetLocationTime() 
        {
            string IpAddress = _accessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            string token = _config.GetSection("TimeZoneApiKey").Value;

            if (IpAddress == "::1")
            {
                IpAddress = "82.26.153.21";
            }

            DateTime dateTime = await _apiService.GetLocationTime(IpAddress, token);

            return dateTime;
        }

        public async Task<Country> GetLocation() 
        {
            string IpAddress = _accessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            string key = _config.GetSection("LocationApiKey").Value;

            if (IpAddress == "::1")
            {
                IpAddress = "82.26.153.21";
            }

            Models.Localisation.Api.Location location = await _apiService.GetLocation(IpAddress, key);

            return new Country() {
                Name = location.country,
                CountryCode = location.countryCode
            };
        }
    }
}
