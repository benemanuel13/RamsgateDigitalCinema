using CinemaWinClient.Models.ApiModels.Localisation;
using Newtonsoft.Json;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models.Localisation;
using RamsgateDigitalCinema.Models.Localisation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Services
{
    public class ApiService : IApiService
    {
        public async Task<CountryRegion> GetCountryRegion(Country country)
        {
            HttpClient client = new HttpClient();

            var resp = await client.GetAsync("https://restcountries.eu/rest/v2/alpha/" + country.CountryCode);

            if (resp.IsSuccessStatusCode)
            {
                string json = await resp.Content.ReadAsStringAsync();

                CountryRegion region = JsonConvert.DeserializeObject<CountryRegion>(json);

                return region;
            }

            return null;
        }

        public async Task<DateTime> GetLocationTime(string IPAddress, string token)
        {
            HttpClient client = new HttpClient();

            var resp = await client.GetAsync("https://timezoneapi.io/api/ip/?" + "ip=" + IPAddress + "&token=" + token);

            if (resp.IsSuccessStatusCode)
            {
                string json = await resp.Content.ReadAsStringAsync();

                Models.Localisation.TimeZoneInfo info = JsonConvert.DeserializeObject<Models.Localisation.TimeZoneInfo>(json);

                return info.data.datetime.DateTime;
            }

            return default(DateTime);
        }

        public async Task<Location> GetLocation(string IPAddress, string token)
        {
            HttpClient client = new HttpClient();

            var resp = await client.GetAsync("https://pro.ip-api.com/json/" + IPAddress + "?key=" + token);

            if (resp.IsSuccessStatusCode)
            {
                string json = await resp.Content.ReadAsStringAsync();

                Location location = JsonConvert.DeserializeObject<Location>(json);

                return location;
            }

            return default(Location);
        }
    }
}
