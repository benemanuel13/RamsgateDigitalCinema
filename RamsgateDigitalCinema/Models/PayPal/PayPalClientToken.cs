using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.PayPal
{
    public class PayPalClientToken
    {
        public string client_id { get; set; }
        public string client_token { get; set; }
        public int expires_in { get; set; }

        public int FilmID { get; set; }
    }
}
