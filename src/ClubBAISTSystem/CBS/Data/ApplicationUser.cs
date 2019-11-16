namespace CBS.Data
{
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        [Microsoft.AspNetCore.Identity.PersonalData]
        public string MemberNumber { get; set; }
    }
}