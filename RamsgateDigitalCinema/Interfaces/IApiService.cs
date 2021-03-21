using CinemaWinClient.Models.ApiModels.Localisation;
using RamsgateDigitalCinema.Models.Localisation;
using RamsgateDigitalCinema.Models.Localisation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Interfaces
{
    public interface IApiService
    {
        Task<CountryRegion> GetCountryRegion(Country country);

        Task<DateTime> GetLocationTime(string IPAddress, string token);
        Task<Location> GetLocation(string IPAddress, string token);
    }
}
