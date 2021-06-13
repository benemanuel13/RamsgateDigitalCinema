using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
using RamsgateDigitalCinema.Extensions;
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

        public MembersController(IEmailSender emailSender, IConfiguration config, IApiService apiService, UserManager<IdentityUser> userManager, IHttpContextAccessor accessor, ApplicationDbContext context) : base(emailSender, config, apiService, userManager, accessor, context)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult AwardsScreen()
        {
            return RedirectToAction("Screen", new { screen = Models.Entities.Screen.Screen1 });
        }

        public async Task<IActionResult> Screen(Screen screen)
        {
            DateTime currentTime = await GetLocationTime();

            ViewBag.CurrentTime = currentTime.ToString("dd/MM/yyyy HH:mm");

            var nextFilm = db.Films.Include(f => f.FilmCollection).Include(f => f.FilmCategory).Join(db.FilmDetails, f => f.FilmID, fd => fd.FilmID, (f, fd) => new ScreenFilmDetails() { Film = f, FilmDetails = fd }).Where(
                    x => x.FilmDetails.Screen == screen && x.Film.Showing > currentTime && (x.Film.FilmCategory.IsViewable || x.Film.FilmID > 239)).OrderBy(x => x.Film.Showing).Where(x => (x.Film.FilmCollectionID == null || x.Film.FilmCollectionID == 0) || x.Film.Title == "Collection").FirstOrDefault();

            if (nextFilm == null)
            { 
                return RedirectToAction("EmptyScreen", routeValues: new { screen = screen });
            }

            nextFilm.Blocked = await IsBlocked(nextFilm.Film.FilmID);
            nextFilm.Screen = (int)screen + 1;

            var title = nextFilm.Film.Title == "Collection" ? db.FilmCollections.Where(fc => fc.FilmID == nextFilm.Film.FilmID).First().Name : nextFilm.Film.Title;
            nextFilm.Title = title;

            nextFilm.Booked = db.MemberFilms.Where(mf => mf.MemberID == CurrentMember.MemberID && mf.FilmID == nextFilm.Film.FilmID).Any();

            return View(nextFilm);
        }

        public ActionResult NotReady()
        {
            return View();
        }

        public ActionResult EmptyScreen(Screen screen)
        {
            EmptyScreenViewModel vm = new EmptyScreenViewModel() {
                Screen = (int)screen + 1
            };

            return View(vm);
        }

        public async Task<bool> IsBlocked(int filmID)
        {
            Country theirCountry = await GetLocation();
            int countryID = db.Countries.Where(c => c.CountryCode == theirCountry.CountryCode).First().CountryID;

            bool any = db.BlockedFilms.Where(bf => bf.FilmID == filmID).Any();
            bool blocked = false;

            if (any)
            {
                blocked = !db.BlockedFilms.Where(bf => bf.FilmID == filmID && bf.CountryID == countryID).Any();
            }

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

            string token = memberFilm.Token;

            if (token == null)
            {
                token = GetMemberFilmToken(memberFilm.FilmID, memberFilm.MemberID);
            }

            ShowFilmViewModel vm = new ShowFilmViewModel() { 
                FilmID = film.FilmID,
                MemberID = CurrentMember.MemberID,
                Film = film,
                Token = token,
                Source = film.Source
            };

            film.Watched++;
            db.SaveChanges();

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> PurchaseTicket(int FilmID, decimal Amount)
        {
            if (Amount > 0)
            {
                Donation donation = new Donation() { FilmID = FilmID, Amount = Amount, MemberID = CurrentMember.MemberID };

                db.Donations.Add(donation);
                db.SaveChanges();
            }

            Film film = db.Films.Include(f => f.FilmCollection).Where(f => f.FilmID == FilmID).First();
            int memberID = CurrentMember.MemberID;

            bool booked = BookPurchase(FilmID);

            if (booked)
            {
                
            }

            if (FilmID == 243)
            {
                booked = BookPurchase(84);
            }

            var user = await _userManager.FindByIdAsync(LoggedInUserASPID);
            string email = user.Email;

            string title = film.Title;
            string screen = "";

            if (film.FilmDetails != null)
            {
                film.FilmDetails.Screen.GetDescription();
            }

            FilmCollection col = db.FilmCollections.Where(fc => fc.FilmID == film.FilmID).FirstOrDefault();

            if (col != null)
            {
                title = col.Name;
            }

            string message = "<p>Thank you for booking '" + title + "'</p><p>Showing: " + film.Showing.ToString("dddd dd MMMM yyyy HH:mm") + "</p><p>Screen: " + screen + "</p>";

            if (film.FilmID == 243)
            {
                message += "<p>Thank you for booking The Opening Night Presentation with the film Adverse and the live Q&A. <br/>Go to www.ramsgatedigitalcinema.co.uk to watch the Presentation and the film.<br/>Here’s the link to join the live Q&A after the screening:<br/>Topic: Q & A Adverse<br/>Time: Jun 3, 2021 09:15 PM London<br/>Doin Zoom Meeting<br/><a href='https://zoom.us/j/96259752381?pwd=K0t5WUFTSHo2bEZYbzEyZWdpRGNWQT09'>https://zoom.us/j/96259752381?pwd=K0t5WUFTSHo2bEZYbzEyZWdpRGNWQT09</a><br/>Meeting ID: 962 5975 2381<br/>Passcode: 943932<br/><br/>Enjoy the Festival<br/>The team at Ramsgate International Film & TV Festival 2021</p>";
            }
            else if (film.FilmID == 244)
            {
                message += "<p>Thank you for booking the CODUMENTARY – IN CONVERSATION WITH JONATHAN BEALES event with the Screening of the film <br/>First, here’s the link to join the live Q&A after the screening: <br/><br/>Topic: CODumentary Master Class <br /> Time: Jun 4, 2021 10:30 AM London<br/><br /> Join Zoom Meeting<br/><a href = 'https://zoom.us/j/94669303654?pwd=clc4S1FOVXZjOFIzcGl1TWpmd1JpUT09'>https://zoom.us/j/94669303654?pwd=clc4S1FOVXZjOFIzcGl1TWpmd1JpUT09</a><br/><br/>Meeting ID: 946 6930 3654 <br /> Passcode: 325456 <br /><br /> Then when the talk is over, go to www.ramsgatedigitalcinema.co.uk to watch the film.<br /><br /> Enjoy the Festival<Br/><br /> The team at Ramsgate International Film & TV Festival 2021 </p> ";
            }
            else if (film.FilmID == 245)
            {
                message += "<p>Thank you for booking the HOW TO GET AN AGENT, WHEN YOU’RE AN ACTOR event <br/>Here’s the link on how to join the live talk:<br/><br/>Topic: How To Get An Agent<br/><br/>Time: Jun 4, 2021 03:00 PM London<br/><br/>Join Zoom Meeting <a href='https://zoom.us/j/91342663574?pwd=ZkpkVEx4MERjMnNOdTdsZHoyTXZkQT09'>https://zoom.us/j/91342663574?pwd=ZkpkVEx4MERjMnNOdTdsZHoyTXZkQT09</a><br/><br/>Meeting ID: 913 4266 3574<br/>Passcode: 216213<br/><br/>Enjoy the Festival<br/><br/>The team at Ramsgate International Film & TV Festival 2021<br/></p>";
            }
            else if (film.FilmID == 246)
            {
                message += "<p>Thank you for booking to attend the NETWORKING PARTY <br/><br/>Here’s the link on how to join the party:<br/>Topic: Networking Party<br/>Time: Jun 4, 2021 06:00 PM London<br/><br/>Join Zoom Meeting<br/><a href='https://zoom.us/j/96017385019?pwd=UEpjRnFXUmFLZjkxR3FVcGFVcFl6dz09'>https://zoom.us/j/96017385019?pwd=UEpjRnFXUmFLZjkxR3FVcGFVcFl6dz09</a><br/><br/>Meeting ID: 960 1738 5019<br/>Passcode: 886708<br/><br/>Enjoy the Festival<br/>The team at Ramsgate International Film & TV Festival 2021<br/></p>";
            }
            else if (film.FilmID == 186)
            {
                message += "<p>Thank you for booking the screening of PHIL LIGGETT: THE VOICE OF CYCLING and the live Q&A. <br/><br/>Go to www.ramsgatedigitalcinema.co.uk to watch the Film<br/><br/>Then, here’s the link to join the live Q&A after the screening:<br/><br/>Topic: Phil Liggett: the Voice of Cycling Q & A<br/><br/>Time: Jun 5, 2021 08:15 PM London<br/><br/>Join Zoom Meeting<br/><br/><a href='https://zoom.us/j/99575075739?pwd=L1BIbTh5dUZPUXBLVTkyVy81M25kUT09'>https://zoom.us/j/99575075739?pwd=L1BIbTh5dUZPUXBLVTkyVy81M25kUT09</a><br/><br/>Meeting ID: 995 7507 5739<br/>Passcode: 949960</p>";
            }
            else if (film.FilmID == 249)
            {
                message += "<p>Thank you for booking TONTON MANU the live Q&A. <br/><br/>Go to www.ramsgatedigitalcinema.co.uk to watch the Film<br/><br/>Then, here’s the link to join the live Q&A after the screening:<br/><br/>Topic: Tonton Manu Q & A<br/><br/>Time: Jun 6, 2021 07:15 PM London<br/><br/>Join Zoom Meeting<br/><a href='https://zoom.us/j/99362988485?pwd=cTlERTFFSGhpaExsdmp3d0duRm4vUT09'>https://zoom.us/j/99362988485?pwd=cTlERTFFSGhpaExsdmp3d0duRm4vUT09</a><br/>Meeting ID: 993 6298 8485<br/>Passcode: 402477<br/></p>";
            }
            else if (film.FilmID == 250)
            {
                message += "<p>Thank you for booking THE AWARDS PRESENTATION  <br/><br/>Here’s the link to join us:<br/><br/>Topic: Awards Presentation<br/>Time: Jun 6, 2021 08:30 PM London<br/><br/>Join Zoom Meeting<br/><a href='https://zoom.us/j/97045349594?pwd=QUFRQk9qWTdhN0cwTFJ0SUJQVU9mdz09'>https://zoom.us/j/97045349594?pwd=QUFRQk9qWTdhN0cwTFJ0SUJQVU9mdz09</a><br/><br/>Meeting ID: 970 4534 9594<br/>Passcode: 523394<br/>Enjoy the Festival<br/><br/>The team at Ramsgate International Film & TV Festival 2021</p>";
            }

            await _emailSender.SendEmailAsync(email, "Booked Film", message);

            return Content("SUCCESS");
        }

        private bool BookPurchase(int FilmID)
        {
            int memberID = CurrentMember.MemberID;

            Film film = db.Films.Include(f => f.FilmCollection).Where(f => f.FilmID == FilmID).First();

            film.Booked++;

            if (film.FilmCollection != null)
            {
                FilmID = film.FilmCollection.FilmID;
            }

            MemberFilm memberFilm = db.MemberFilms.Where(mf => mf.FilmID == FilmID && mf.MemberID == memberID).FirstOrDefault();

            if (memberFilm != null && memberFilm.Token != null)
            {
                return true;
            }

            if (memberFilm != null && memberFilm.Token == null)
            {
                db.MemberFilms.Remove(memberFilm);
                db.SaveChanges();
            }

            MemberFilm mf = new MemberFilm() { FilmID = FilmID, MemberID = memberID };

            db.MemberFilms.Add(mf);
            db.SaveChanges();

            try
            {
                GetMemberFilmToken(FilmID, memberID);
            }
            catch { }

            return false;
        }
        public async Task<ActionResult> BookFilm(int id)
        {
            var mf = db.MemberFilms.Where(m => m.FilmID == id && m.MemberID == CurrentMember.MemberID && m.Token != null).Any();

            if (mf)
            {
                return RedirectToAction("AlreadyBooked", "Members");
            }

            bool blocked = await IsBlocked(id);

            if (blocked)
            {
                return RedirectToAction("GeoBlocked");
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

            if (film.FilmID == 242 || film.FilmID == 244 || film.FilmID == 245 || film.FilmID == 248)
            {
                ViewBag.Fiver = true;
            }
            else
            {
                ViewBag.Fiver = false;
            }

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

        public async Task<ActionResult> BookedFilms()
        {
            var films = db.MemberFilms.Join(db.Films, mf => mf.FilmID, f => f.FilmID, (mf, f) => new { MemberFilm = mf, Film = f }).Where(x => x.MemberFilm.MemberID == CurrentMember.MemberID && x.Film.Showing > DateTime.Parse("2021-06-10"))
                .Select(x => new BookedFilmViewModel() { 
                    FilmID = x.Film.FilmID,
                    Title = db.FilmCollections.Where(fc => fc.FilmID == x.Film.FilmID).FirstOrDefault() == null ? x.Film.Title : db.FilmCollections.Where(fc => fc.FilmID == x.Film.FilmID).FirstOrDefault().Name,
                    Showing = x.Film.Showing,
                    IsExtra = x.Film.FilmCategoryID == 26
                }).OrderBy(x => x.Showing).ToList();

            ViewBag.CurrentTime = await GetLocationTime();

            return View(films);
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
                expires: DateTime.Now.AddDays(7),
                signingCredentials: cred);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }
        #endregion

        public ActionResult Questionnaire(int filmID)
        {
            QuestionnaireViewModel vm = new QuestionnaireViewModel() { 
                MemberID = CurrentMember.MemberID,
                FilmID = filmID,
                IsCollection = false
            };

            var collection = db.FilmCollections.Where(fc => fc.FilmID == filmID).FirstOrDefault();

            if (collection != null)
            {
                vm.IsCollection = true;
                vm.CollectionFilms = db.Films.Where(f => f.FilmCollectionID == collection.FilmCollectionID).ToList();
            }

            return View(vm);
        }

        [HttpPost]
        public ActionResult Questionnaire(QuestionnaireViewModel vm)
        {

            return View();
        }
    }
}
