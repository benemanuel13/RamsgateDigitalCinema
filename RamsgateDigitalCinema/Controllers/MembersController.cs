using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Interfaces;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.Models.Localisation;
using RamsgateDigitalCinema.Models.PayPal;
using RamsgateDigitalCinema.ViewModels.Members;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers
{
    [Authorize]
    public class MembersController : BaseController
    {
        private static Dictionary<string, string> KeyIdentifiers = new Dictionary<string, string>();
        static readonly object _object = new object();

        private string ContentKeyPolicyName = "SharedContentKeyPolicyUsedByAllAssets";
        private string Issuer = "myIssuer";
        private string Audience = "myAudience";

        public MembersController(IConfiguration config, IApiService apiService, UserManager<IdentityUser> userManager, IHttpContextAccessor accessor, ApplicationDbContext context) : base(config, apiService, userManager, accessor, context)
        {

        }

        public IActionResult Index()
        {
            if (CurrentMember.Role == MemberRole.User)
            {
                return RedirectToAction("NotReady");
            }

            return View();
        }

        public async Task<IActionResult> Screen(Screen screen)
        {
            if (CurrentMember.Role == MemberRole.User)
            {
                return RedirectToAction("NotReady");
            }

            DateTime currentTime = await GetLocationTime();

            var nextFilm = db.Films.Include(f => f.FilmCollection).Include(f => f.FilmCategory).Join(db.FilmDetails, f => f.FilmID, fd => fd.FilmID, (f, fd) => new ScreenFilmDetails() { Film = f, FilmDetails = fd }).Where(
                    x => x.FilmDetails.Screen == screen && x.Film.Showing > currentTime && x.Film.FilmCategory.IsViewable).OrderBy(x => x.Film.Showing).FirstOrDefault();

            if (nextFilm == null)
            { 
                return RedirectToAction("EmptyScreen", routeValues: new { screen = screen });
            }

            nextFilm.Blocked = await IsBlocked(nextFilm.Film.FilmID);

            return View(nextFilm);
        }

        public ActionResult NotReady()
        {
            return View();
        }

        public ActionResult EmptyScreen(Screen screen)
        {
            return View();
        }

        public async Task<bool> IsBlocked(int filmID)
        {
            Country theirCountry = await GetLocation();
            int countryID = db.Countries.Where(c => c.CountryCode == theirCountry.CountryCode).First().CountryID;

            bool blocked = db.BlockedFilms.Where(bf => bf.FilmID == filmID && bf.CountryID == countryID).Any();

            return blocked;
        }

        public ActionResult NoSuchFilm()
        {
            return View();
        }

        public ActionResult GeoBlocked()
        {
            return View();
        }

        public ActionResult NotBooked()
        {
            return View();
        }

        public async Task<ActionResult> ShowFilm(int id)
        {
            Film film = db.Films.Where(f => f.FilmID == id).FirstOrDefault();
            film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == id).FirstOrDefault();

            if (film == null)
            {
                return RedirectToAction("NoSuchFilm");
            }

            bool blocked = await IsBlocked(id);

            if (blocked)
            {
                return RedirectToAction("GeoBlocked");
            }

            MemberFilm memberFilm = db.MemberFilms.Where(mf => mf.FilmID == id && mf.MemberID == CurrentMember.MemberID).FirstOrDefault();

            if (memberFilm == null)
            {
                return RedirectToAction("NotBooked");
            }

            ShowFilmViewModel vm = new ShowFilmViewModel() { 
                FilmID = film.FilmID,
                MemberID = CurrentMember.MemberID,
                Film = film,
                Token = memberFilm.Token,
                Source = film.Source
            };

            film.Watched++;
            db.SaveChanges();

            return View(vm);
        }

        [HttpPost]
        public ActionResult PurchaseTicket(int FilmID, decimal Amount)
        {
            if (Amount > 0)
            {
                Donation donation = new Donation() { FilmID = FilmID, Amount = Amount, MemberID = CurrentMember.MemberID };

                db.Donations.Add(donation);
                db.SaveChanges();
            }

            int memberID = CurrentMember.MemberID;

            Film film = db.Films.Include(f => f.FilmCollection).Where(f => f.FilmID == FilmID).First();

            film.Booked++;

            if (film.FilmCollection != null)
            {
                FilmID = film.FilmCollection.FilmID;
            }

            MemberFilm memberFilm = db.MemberFilms.Where(mf => mf.FilmID == FilmID && mf.MemberID == memberID).FirstOrDefault();

            if (memberFilm != null)
            {
                return Content("SUCCESS");
            }

            MemberFilm mf = new MemberFilm() { FilmID = FilmID, MemberID = memberID };

            db.MemberFilms.Add(mf);
            db.SaveChanges();

            GetMemberFilmToken(FilmID, memberID);

            return Content("SUCCESS");
        }

        public async Task<ActionResult> BookFilm(int id)
        {
            var mf = db.MemberFilms.Where(m => m.FilmID == id && m.MemberID == CurrentMember.MemberID).Any();

            if (mf)
            {
                return RedirectToAction("AlreadyBooked", "Members");
            }

            Film film = db.Films.Find(id);

            PayPalToken authToken = GetAuthorizationToken();
            PayPalClientToken clientToken = null;

            PayPalDetails details = db.PayPalDetails.First();

            if (authToken.Access_Token == "Failed")
            {
                clientToken = new PayPalClientToken() { client_id = details.ClientID, client_token = "FailedAuthorization" };
            }
            else
            {
                clientToken = GetClientToken(authToken.Access_Token);
                clientToken.client_id = details.ClientID;
            }

            BookFilmViewModel vm = new BookFilmViewModel() { 
                FilmID = id,
                ClientID = clientToken.client_id,
                ClientToken = clientToken.client_token
            };

            if (film != null)
            {
                ViewBag.ShowingTime = film.Showing;
            }

            ViewBag.CurrentTime = await GetLocationTime();

            return View(vm);
        }

        private PayPalClientToken GetClientToken(string auth)
        {
            HttpClient client = new HttpClient();

            PayPayClient ppClient = new PayPayClient() { customer_id = "customer_" + CurrentMember.MemberID };
            string ppJson = JsonConvert.SerializeObject(ppClient);

            var bytes = Encoding.UTF8.GetBytes(auth);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + auth);
            client.DefaultRequestHeaders.Add("Accept-Language", "en_US");

            StringContent content = new StringContent(ppJson, UTF8Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync("https://api.paypal.com/v1/identity/generate-token", content).Result;

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                PayPalClientToken cliToken = JsonConvert.DeserializeObject<PayPalClientToken>(json);

                return cliToken;
            }

            return new PayPalClientToken() { client_token = "FailedClient" };
        }

        private PayPalToken GetAuthorizationToken()
        {
            PayPalDetails details = db.PayPalDetails.First();
            PayPalToken token = null;

            if (details.Expires < DateTime.Now)
            {
                token = GetSetPayPalToken();
            }
            else
            {
                token = new PayPalToken() { Access_Token = details.Token };
            }

            return token;
        }

        public PayPalToken GetSetPayPalToken()
        {
            PayPalDetails details = db.PayPalDetails.First();

            string ClientID = details.ClientID;
            string Secret = details.Secret;

            HttpClient client = new HttpClient();
            StringContent content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var bytes = Encoding.UTF8.GetBytes(ClientID + ":" + Secret);
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(bytes));

            HttpResponseMessage response = client.PostAsync("https://api.paypal.com/v1/oauth2/token", content).Result;

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                PayPalToken token = JsonConvert.DeserializeObject<PayPalToken>(json);

                int seconds = int.Parse(token.Expires_In);

                details.Token = token.Access_Token;
                details.Expires = DateTime.Now.AddSeconds(seconds);
                db.SaveChanges();

                return token;
            }
            else
            {
                return new PayPalToken() { Access_Token = "Failed" };
            }
        }

        #region SetSources (defunct)

        public ActionResult SetSources()
        {
            var client = CreateClient().Result;

            StreamingEndpoint streamingEndpoint = client.StreamingEndpoints.GetAsync(_config.GetSection("ResourceGroup").Value, _config.GetSection("AccountName").Value, "default").Result;

            var filmsToSource = db.Films.Where(f => f.AssetCreated && (f.Source == null || f.Source == "")).ToList();

            foreach (Film film in filmsToSource)
            {
                ListPathsResponse paths = client.StreamingLocators.ListPathsAsync(_config.GetSection("ResourceGroup").Value, _config.GetSection("AccountName").Value, film.AssetName + "Locator").Result;

                string dashPath = "";

                foreach (StreamingPath path in paths.StreamingPaths)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "https";
                    uriBuilder.Host = streamingEndpoint.HostName;

                    // Look for just the DASH path and generate a URL for the Azure Media Player to playback the content with the AES token to decrypt.
                    // Note that the JWT token is set to expire in 1 hour. 
                    if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
                    {
                        uriBuilder.Path = path.Paths[0];

                        dashPath = uriBuilder.ToString();
                    }
                }

                SetSource(film.FilmID, dashPath);
            }

            return Content("OK");
        }

        public void SetSource(int filmID, string source)
        {
            var film = db.Films.Find(filmID);
            film.Source = source;
            //film.Streaming = true;

            db.SaveChanges();
        }
        #endregion

        #region TokenStuff

        public string GetMemberFilmToken(int filmID, int memberID)
        {
            string token = GetToken(filmID, 2, 360).Result;

            var memberFilm = db.MemberFilms.Where(mf => mf.FilmID == filmID && mf.MemberID == memberID).First();
            memberFilm.Token = token;

            db.SaveChanges();

            return token;
        }

        private async Task<IAzureMediaServicesClient> CreateClient()
        {
            IAzureMediaServicesClient client = await CreateMediaServicesClientAsync();

            return client;
        }

        private async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync()
        {
            var credentials = await GetCredentialsAsync();

            return new AzureMediaServicesClient( new Uri(_config.GetSection("ArmEndpoint").Value), credentials)
            {
                SubscriptionId = _config.GetSection("SubscriptionId").Value
            };
        }

        private async Task<ServiceClientCredentials> GetCredentialsAsync()
        {
            ClientCredential clientCredential = new ClientCredential(_config.GetSection("AadClientId").Value, _config.GetSection("AadSecret").Value);
            return await ApplicationTokenProvider.LoginSilentAsync(_config.GetSection("AadTenantId").Value, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }


        private async Task<string> GetToken(int filmID, int numUses, int expireMinutes)
        {
            Film film = db.Films.Find(filmID);
            string locatorName = film.AssetName;

            db.SaveChanges();

            string token = _config.GetSection("TokenKey").Value;

            IAzureMediaServicesClient azureclient = await CreateClient();

            //RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            //byte[] bytes = new byte[40];
            //rng.GetBytes(bytes);
            //rng.Dispose();
            byte[] bytes = System.Convert.FromBase64String(token);

            if (azureclient == null)
            {
                azureclient = await CreateClient();
            }

            string keyIdentifier;

            lock (_object)
            {
                if (!KeyIdentifiers.ContainsKey(locatorName))
                {
                    keyIdentifier = azureclient.StreamingLocators.ListContentKeys(_config.GetSection("ResourceGroup").Value, _config.GetSection("AccountName").Value, locatorName + "Locator").ContentKeys.First().Id.ToString();
                    KeyIdentifiers.TryAdd(locatorName, keyIdentifier);
                }
                else
                {
                    keyIdentifier = KeyIdentifiers[locatorName];
                }
            }

            string endtoken = GetTokenAsync(film, Issuer, Audience, keyIdentifier, bytes, numUses, expireMinutes);

            return endtoken;
        }

        private string GetTokenAsync(Film film, string issuer, string audience, string keyIdentifier, byte[] tokenVerificationKey, int numberUses, int expireMinutes)
        {
            var tokenSigningKey = new SymmetricSecurityKey(tokenVerificationKey);

            SigningCredentials cred = new SigningCredentials(
                tokenSigningKey,
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest);

            string claim = "urn:microsoft:azure:mediaservices:maxuses";

            Claim[] claims = new Claim[]
            {
                new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, keyIdentifier),
                new Claim(claim, numberUses.ToString())
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.Now.AddDays(-1),
                //expires: film.Showing.AddDays(2),
                expires: DateTime.Now.AddDays(2),
                signingCredentials: cred);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }
        #endregion
    }
}
