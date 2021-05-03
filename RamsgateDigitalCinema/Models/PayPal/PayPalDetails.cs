using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.PayPal
{
    public class PayPalDetails
    {
        public int PayPalDetailsID { get; set; }

        public string ClientID { get; set; }
        public string Secret { get; set; }

        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
