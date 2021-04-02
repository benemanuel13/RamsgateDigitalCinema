using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public class Donation
    {
        public int DonationID { get; set; }

        public int MemberID { get; set; }
        public int FilmID { get; set; }

        public decimal Amount { get; set; }
    }
}
