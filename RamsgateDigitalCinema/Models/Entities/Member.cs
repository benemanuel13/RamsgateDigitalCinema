using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Models.Entities
{
    public enum MemberRole
    {
        User,
        Tester,
        Admin
    }

    public class Member
    {
        public int MemberID { get; set; }

        public string ASPID { get; set; }

        public MemberRole Role { get; set; }

        public TimeSpan TimeDiff { get; set; }
        public bool TimeDiffPositive { get; set; }

        public DateTime LastLoggedIn { get; set; }

        public bool DontEmail { get; set; }

        public string UnsubscribeCode { get; set; }

        public string ApiKey { get; set; }
    }
}
