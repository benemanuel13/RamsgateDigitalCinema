using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.PayPal
{
    public class PayPalToken
    {
        public string Scope { get; set; }
        public string Access_Token { get; set; }
        public string Token_Type { get; set; }
        public string App_Id { get; set; }
        public string Expires_In { get; set; }
        public string Nonce { get; set; } 
    }
}
