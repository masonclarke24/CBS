namespace CBS
{
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        [Microsoft.AspNetCore.Identity.PersonalData]
        public string MembershipLevel { get; set; }
    }
}