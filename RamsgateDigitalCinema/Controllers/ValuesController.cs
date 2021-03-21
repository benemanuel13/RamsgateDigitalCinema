using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IApiService _apiService;

        public ValuesController(IApiService apiService, ApplicationDbContext context)
        {
            db = context;
            _apiService = apiService;
        }

        [HttpPost("PostCountry")]
        public async Task<string> PostCountry(Country country)
        {
            var original = db.Countries.Where(c => c.CountryCode == country.CountryCode).FirstOrDefault();

            if (original == null)
            {
                var countryRegion = await _apiService.GetCountryRegion(country);

                var region = db.Regions.Where(r => r.Name == countryRegion.region).FirstOrDefault();

                if (region == null)
                {
                    region = new Region() {
                        Name = countryRegion.region
                    };

                    db.Regions.Add(region);
                    db.SaveChanges();
                }

                var subRegion = db.SubRegions.Where(sr => sr.Name == countryRegion.subregion).FirstOrDefault();

                if (subRegion == null)
                {
                    subRegion = new SubRegion() { 
                        Name = countryRegion.subregion,
                        RegionID = region.RegionID
                    };

                    db.SubRegions.Add(subRegion);
                    db.SaveChanges();
                }

                country.SubRegionID = subRegion.SubRegionID;
                db.Countries.Add(country);
                db.SaveChanges();

                return "OK";
            }

            return "ALREADY_EXISTS";
        }
    }
}
