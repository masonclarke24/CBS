﻿using System.Linq;

namespace CBS.Data
{
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {

        [Microsoft.AspNetCore.Identity.PersonalData]
        public string MemberNumber { get; set; }
        [Microsoft.AspNetCore.Identity.PersonalData]
        public string MemberName { get; set; }

    }
}