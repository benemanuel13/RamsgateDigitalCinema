using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RamsgateDigitalCinema.Data;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.Models.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Rest;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RamsgateDigitalCinema.Services;

namespace RamsgateDigitalCinema.Controllers
{
    public enum FileType
    {
        Trailer,
        Poster,
        Still,
        Director
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        IConfiguration config;

        private static Dictionary<string, string> KeyIdentifiers = new Dictionary<string, string>();
        static readonly object _object = new object();

        private string Issuer = "myIssuer";
        private string Audience = "myAudience";

        public AdminController(IConfiguration config, ApplicationDbContext context)
        {
            db = context;
            this.config = config;
        }

        [HttpGet("GetFilmCategories")]
        public List<FilmCategory> GetFilmCategories()
        {
            var cats = db.FilmCategories.ToList();

            return cats;
        }

        [HttpPost("PostFilmCategory")]
        public FilmCategory PostFilmCategory(FilmCategory cat)
        {
            var original = db.FilmCategories.Find(cat.FilmCategoryID);

            if (original == null)
            {
                db.FilmCategories.Add(cat);
                db.SaveChanges();

                return cat;
            }

            original.Description = cat.Description;
            original.IsViewable = cat.IsViewable;
            db.SaveChanges();

            return original;
        }

        [HttpGet("GetFilmCollections")]
        public List<FilmCollection> GetFilmCollections()
        {
            var cols = db.FilmCollections.ToList();

            foreach (var collection in cols)
            {
                collection.Film = db.Films.Include(f => f.FilmCollection).Include(f => f.FilmCategory).Where(f => f.FilmID == collection.FilmID).FirstOrDefault();
            }

            return cols;
        }

        [HttpPost("PostFilmCollection")]
        public FilmCollection PostFilmCollection(FilmCollection col)
        {
            var original = db.FilmCollections.Find(col.FilmCollectionID);

            if (original == null)
            {
                db.FilmCollections.Add(col);
                db.SaveChanges();

                FilmCategory cat = db.FilmCategories.Where(fc => fc.Description == Film.SHORT_COLLECTION).FirstOrDefault();
                Film newFilm = new Film()
                {
                    Title = col.Name,
                    Showing = DateTime.Parse("03/06/2021 11:00"),
                    FilmCategoryID = cat.FilmCategoryID,
                    FilmCollectionID = col.FilmCollectionID
                };

                db.Films.Add(newFilm);
                db.SaveChanges();

                col.FilmID = newFilm.FilmID;
                col.Film = newFilm;
                db.SaveChanges();

                return col;
            }

            original.Name = col.Name;
            db.SaveChanges();

            return original;
        }

        [HttpGet("GetFilms")]
        public List<Film> GetFilms(int id)
        {
            List<Film> films = null;

            if (id == 0)
            {
                films = db.Films.Include(f => f.FilmCategory).Include(f => f.FilmCollection).Where(f => f.FilmCollectionID == null).ToList();
            }
            else
            {
                films = db.Films.Include(f => f.FilmCategory).Include(f => f.FilmCollection).Where(f => f.FilmCollectionID == id).ToList();
            }

            foreach (var film in films)
            {
                film.FilmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();
            }

            return films;
        }

        [HttpPost("PostFilm")]
        public Film PostFilm(Film film)
        {
            var original = db.Films.Find(film.FilmID);

            if (original == null)
            {
                db.Films.Add(film);
                db.SaveChanges();

                return film;
            }

            original.Title = film.Title;
            original.Director = film.Director;
            original.Rating = film.Rating;
            original.Showing = film.Showing;
            original.Uploaded = film.Uploaded;
            original.RemoteFileName = film.RemoteFileName;
            original.AssetCreated = film.AssetCreated;

            original.AssetName = film.AssetName;

            original.FilmCollectionID = film.FilmCollectionID;
            original.FilmCategoryID = film.FilmCategoryID;

            db.SaveChanges();

            return original;
        }

        [HttpGet("GetRegions")]
        public List<Region> GetRegions()
        {
            var regions = db.Regions.ToList();

            return regions;
        }

        [HttpGet("GetSubRegions")]
        public List<SubRegion> GetSubRegions(int id)
        {
            var subRegions = db.SubRegions.Where(sr => sr.RegionID == id).ToList();

            return subRegions;
        }

        [HttpGet("GetCountries")]
        public List<Country> GetCountries(int id)
        {
            var countries = db.Countries.Where(c => c.SubRegionID == id).ToList();

            return countries;
        }

        [HttpGet("GetFilmDetails")]
        public FilmDetails GetFilmDetails(int id)
        {
            Film film = db.Films.Find(id);
            FilmDetails details = db.FilmDetails.Include(fd => fd.Film).Include(fd => fd.StillUrls).Where(fd => fd.FilmID == id).FirstOrDefault();

            if (details == null)
            {
                details = new FilmDetails()
                {
                    FilmID = id,
                    Film = film,
                    StillUrls = new List<StillUrl>()
                };

                db.FilmDetails.Add(details);
                db.SaveChanges();
            }

            return details;
        }

        [HttpPost("PostFile")]
        [RequestSizeLimit(41201000000)]
        public string PostFile(int id, FileType fileType)
        {
            string message = "OK";

            if (!Request.Form.Files.Any())
            {
                return "Failure";
            }

            string subFolder = "";

            try
            {
                if (fileType == FileType.Trailer)
                {
                    subFolder = "Trailers";
                }
                else if (fileType == FileType.Poster)
                {
                    subFolder = "Posters";
                }
                else if (fileType == FileType.Still)
                {
                    subFolder = "Stills";
                }
                else if (fileType == FileType.Director)
                {
                    subFolder = "Directors";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            string myFileName = "";
            string folder = AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\" + subFolder + "\\";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            try
            {
                IFormFile myFile = Request.Form.Files.First();

                //myFileName = myFile.Name.Replace(" ", "");
                myFileName = Guid.NewGuid().ToString() + Path.GetExtension(myFile.Name);

                FileStream stream = new FileStream(folder + myFileName, FileMode.Create);

                myFile.CopyTo(stream);

                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            Film film = null;
            FilmDetails details = null;

            try
            {
                film = db.Films.Find(id);
                details = db.FilmDetails.Include(fd => fd.Film).Include(fd => fd.StillUrls).Where(fd => fd.FilmID == id).FirstOrDefault();

                if (details == null)
                {
                    details = new FilmDetails()
                    {
                        FilmID = id,
                        Film = film,
                        StillUrls = new List<StillUrl>()
                    };

                    db.FilmDetails.Add(details);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            try
            {
                if (fileType == FileType.Trailer)
                {
                    details.TrailerUrl = "/" + subFolder + "/" + myFileName;
                }
                else if (fileType == FileType.Poster)
                {
                    details.PosterUrl = "/" + subFolder + "/" + myFileName;
                }
                else if (fileType == FileType.Still)
                {
                    details.StillUrls.Add(new StillUrl() { Url = "/" + subFolder + "/" + myFileName });
                }
                else if (fileType == FileType.Director)
                {
                    details.DirectorPicUrl = "/" + subFolder + "/" + myFileName;
                }

                message = "OK" + myFileName;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }


        [HttpPost("UpdateFilmDetails")]
        public string UpdateFilmDetails([FromBody] FilmDetails filmDetails)
        {
            FilmDetails original = db.FilmDetails.Find(filmDetails.FilmDetailsID);

            db.Entry(original).CurrentValues.SetValues(filmDetails);
            db.SaveChanges();

            return filmDetails.FilmDetailsID.ToString();
        }

        [HttpGet("GetFilm")]
        public Film GetFilm(int id)
        {
            Film film = db.Films.Include(f => f.FilmCategory).Include(f => f.FilmCollection).Where(f => f.FilmID == id).FirstOrDefault();

            return film;
        }

        [HttpPost("PostFilmFile")]
        [RequestSizeLimit(41201000000)]
        public string PostFilmFile()
        {
            try
            {
                string folder = AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\" + config.GetSection("UploadPath").Value + "\\";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var memstream = new MemoryStream();
                HttpContext.Request.Body.CopyTo(memstream);

                memstream.Seek(0, SeekOrigin.Begin);

                string myFileName = HttpContext.Request.Headers["Filename"];

                FileStream stream = null;

                string first = HttpContext.Request.Headers["First"];

                if (first == "true")
                {
                    stream = new FileStream(folder + myFileName, FileMode.Create);
                }
                else
                {
                    stream = new FileStream(folder + myFileName, FileMode.Append);
                }

                long bytesCopied = 0;
                long chunkSize = long.Parse(HttpContext.Request.Headers["ChunkSize"]);
                long length = long.Parse(HttpContext.Request.Headers["Length"]);
                byte[] buffer = new byte[chunkSize];

                while (bytesCopied < length)
                {
                    int read = 0;

                    read = memstream.Read(buffer, (int)bytesCopied, (int)(length - bytesCopied));

                    stream.Write(buffer, (int)(bytesCopied), (int)read);

                    bytesCopied += read;
                }

                memstream.Close();

                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        [HttpGet("DeleteFilmFile")]
        public string DeleteFilmFile(int id)
        {
            Film film = db.Films.Find(id);

            string path = AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\" + config.GetSection("UploadPath").Value + "\\" + film.RemoteFileName;

            System.IO.File.Delete(path);

            return "SUCCESS";
        }

        [HttpGet("GetBlockedFilms")]
        public List<Country> GetBlockedFilms(int id)
        {
            var countries = db.Countries.Join(db.BlockedFilms, c => c.CountryID, bf => bf.CountryID, (c, bf) => new { Country = c, FilmID = bf.FilmID }).Where(x => x.FilmID == id).Select(x => x.Country).ToList();

            return countries;
        }

        [HttpPost("PostBlockedCountries")]
        public void PostBlockedCountries(int id, [FromBody] List<Country> countries)
        {
            var blocked = db.BlockedFilms.Where(b => b.FilmID == id).ToList();

            db.BlockedFilms.RemoveRange(blocked);

            foreach (var country in countries)
            {
                BlockedFilm block = new BlockedFilm() { 
                    FilmID = id,
                    CountryID = country.CountryID
                };

                db.BlockedFilms.Add(block);
            }

            db.SaveChanges();
        }

        [HttpGet("GetUploadedFilmList")]
        public string[] GetUploadedFilmList()
        {
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\" + config.GetSection("UploadPath").Value + "\\");

            return files;
        }

        [HttpGet("DeleteUploadedFilms")]
        public string DeleteUploadedFilms()
        {
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "wwwroot\\" + config.GetSection("UploadPath").Value + "\\");

            foreach (string file in files)
            {
                System.IO.File.Delete(file);
            }

            return "OK";
        }

        [HttpGet("SetSources")]
        public ActionResult SetSources()
        {
            var client = CreateClient().Result;

            StreamingEndpoint streamingEndpoint = client.StreamingEndpoints.GetAsync(config.GetSection("ResourceGroup").Value, config.GetSection("AccountName").Value, "default").Result;

            var filmsToSource = db.Films.Where(f => f.AssetCreated && (f.Source == null || f.Source == "")).ToList();

            foreach (Film film in filmsToSource)
            {
                ListPathsResponse paths = client.StreamingLocators.ListPathsAsync(config.GetSection("ResourceGroup").Value, config.GetSection("AccountName").Value, film.AssetName + "Locator").Result;

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

        private async Task<IAzureMediaServicesClient> CreateClient()
        {
            IAzureMediaServicesClient client = await CreateMediaServicesClientAsync();

            return client;
        }

        private async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync()
        {
            var credentials = await GetCredentialsAsync();

            return new AzureMediaServicesClient(new Uri(config.GetSection("ArmEndpoint").Value), credentials)
            {
                SubscriptionId = config.GetSection("SubscriptionId").Value
            };
        }

        private async Task<ServiceClientCredentials> GetCredentialsAsync()
        {
            ClientCredential clientCredential = new ClientCredential(config.GetSection("AadClientId").Value, config.GetSection("AadSecret").Value);
            return await ApplicationTokenProvider.LoginSilentAsync(config.GetSection("AadTenantId").Value, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }

        private async Task<string> GetToken(int filmID, int numUses, int expireMinutes)
        {
            Film film = db.Films.Find(filmID);
            string locatorName = film.AssetName;

            db.SaveChanges();

            string token = config.GetSection("TokenKey").Value;

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
                    keyIdentifier = azureclient.StreamingLocators.ListContentKeys(config.GetSection("ResourceGroup").Value, config.GetSection("AccountName").Value, locatorName + "Locator").ContentKeys.First().Id.ToString();
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

        public string GetMemberFilmToken(int filmID, int memberID)
        {
            string token = GetToken(filmID, 2, 360).Result;

            var memberFilm = db.MemberFilms.Where(mf => mf.FilmID == filmID && mf.MemberID == memberID).First();
            memberFilm.Token = token;

            db.SaveChanges();

            return token;
        }

        [HttpGet("SetTokens")]
        public ActionResult SetTokens()
        {
            var films = db.MemberFilms.Where(t => t.Token == null).ToList();

            foreach (var film in films)
            {
                try
                {
                    GetMemberFilmToken(film.FilmID, film.MemberID);
                }
                catch
                { }
            }

            return Content("OK");
        }

        [HttpGet("SetScreens")]
        public ActionResult SetScreens()
        {
            var collections = db.FilmCollections.ToList();

            int count = 0;

            foreach (var col in collections)
            {
                FilmDetails details = db.FilmDetails.Where(fd => fd.FilmID == col.FilmID).FirstOrDefault();

                if (details != null)
                {
                    Screen screen = details.Screen;

                    var films = db.Films.Where(f => f.FilmCollectionID == col.FilmCollectionID).ToList();

                    foreach (var film in films)
                    {
                        FilmDetails filmDetails = db.FilmDetails.Where(fd => fd.FilmID == film.FilmID).FirstOrDefault();

                        if (filmDetails != null)
                        {
                            filmDetails.Screen = screen;
                            count++;
                        }
                    }
                }
            }

            db.SaveChanges();

            return Content("OK: " + count.ToString());
        }

        [HttpGet("RecreateTokens")]
        public ActionResult RecreateTokens(int filmID)
        {
            var films = db.MemberFilms.Where(mf => mf.FilmID == filmID).ToList();

            int count = 0;

            foreach (var film in films)
            {
                film.Token = GetMemberFilmToken(film.FilmID, film.MemberID);
                db.SaveChanges();

                count++;
            }

            return Content("OK: " + count.ToString());
        }
    }
}
