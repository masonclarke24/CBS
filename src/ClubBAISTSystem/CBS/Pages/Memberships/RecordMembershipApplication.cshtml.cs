using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices;
using TechnicalServices.Memberships;

namespace CBS
{
    [BindProperties, Authorize(Policy = "ManageMemberships")]
    public class RecordMembershipApplicationModel : PageModel
    {
        private const string postalCodeFormat = @"^[A-Z]\d[A-Z] \d[A-Z]\d$";
        private const string phoneFormat = @"\d{10}";
        private const string emailFormat = @"^.+@.+\..+$";
        private const string dateFormatString = "{0:dd/MMM/yyyy}";
        #region MembershipFields
        [Required, MaxLength(25), Display(Name ="Last Name")]
        public string LastName { get; set; }
        [Required, MaxLength(25), Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required, MaxLength(64), Display(Name = "Address")]
        public string ApplicantAddress { get; set; }
        [Required, MaxLength(35), Display(Name = "City")]
        public string ApplicantCity { get; set; }
        [Required, RegularExpression(postalCodeFormat), Display(Name = "Postal Code")]
        public string ApplicantPostalCode { get; set; }
        [Required, RegularExpression(phoneFormat), Display(Name = "Phone")]
        public string ApplicantPhone { get; set; }
        [Required, RegularExpression(phoneFormat), Display(Name = "Alternate Phone")]
        public string ApplicantAlternatePhone { get; set; }
        [Required, RegularExpression(emailFormat)]
        public string Email { get; set; }
        [Required, Display(Name = "Date of Birth"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = dateFormatString), PageRemote(AdditionalFields = "__RequestVerificationToken", HttpMethod = "POST", PageHandler = "CheckDate")]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
        [Required, Display(Name = "Membership Type")]
        public MembershipType MembershipType { get; set; }
        [Required, MaxLength(25)]
        public string Occupation { get; set; }
        [Required, MaxLength(30), Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Required, MaxLength(64), Display(Name = "Employer Address")]
        public string EmployerAddress { get; set; }
        [Required, MaxLength(35), Display(Name = "Employer City")]
        public string EmployerCity { get; set; }
        [Required, RegularExpression(postalCodeFormat), Display(Name = "Employer Postal Code")]
        public string EmployerPostalCode { get; set; }
        [Required, RegularExpression(phoneFormat), Display(Name = "Employer Phone")]
        public string EmployerPhone { get; set; }
        public bool ProspectiveMemberCertification { get; set; }
        [Required, MaxLength(25), Display(Name = "Shareholder 1")]
        public string Shareholder1 { get; set; }
        [Required, MaxLength(25), Display(Name = "Shareholder 2")]
        public string Shareholder2 { get; set; }
        [Required, Display(Name = "Shareholder 1 Signing Date"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = dateFormatString), PageRemote(AdditionalFields = "__RequestVerificationToken", HttpMethod = "POST", PageHandler = "CheckDate")]
        public DateTime Shareholder1SigningDate { get; set; } = DateTime.UtcNow;
        [Required, Display(Name = "Shareholder 2 Signing Date"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = dateFormatString), PageRemote(AdditionalFields = "__RequestVerificationToken", HttpMethod = "POST", PageHandler = "CheckDate")]
        public DateTime Shareholder2SigningDate { get; set; } = DateTime.UtcNow;
        public bool ShareholderCertification { get; set; }
        #endregion
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            #region CreateMembershipApplication
            MembershipApplication membershipApplication = new MembershipApplication();
            membershipApplication.ProspectiveMemberContactInfo = new ContactInformation()
            {
                LastName = LastName,
                FirstName = FirstName,
                Address = ApplicantAddress,
                City = ApplicantCity,
                PostalCode = ApplicantPostalCode,
                PrimaryPhone = ApplicantPhone,
                SecondaryPhone = ApplicantAlternatePhone,
                Email = Email
            };
            membershipApplication.DateOfBirth = DateOfBirth;
            membershipApplication.MembershipType = MembershipType;
            membershipApplication.Occupation = Occupation;
            membershipApplication.EmploymentDetails = new ContactInformation()
            {
                CompanyName = CompanyName,
                Address = EmployerAddress,
                City = EmployerCity,
                PostalCode = EmployerPostalCode,
                PrimaryPhone = EmployerPhone
            };

            membershipApplication.ProspectiveMemberCertification = ProspectiveMemberCertification;
            membershipApplication.Shareholder1 = Shareholder1;
            membershipApplication.Shareholder1SigningDate = Shareholder1SigningDate;
            membershipApplication.Shareholder2 = Shareholder2;
            membershipApplication.Shareholder2SigningDate = Shareholder2SigningDate;
            membershipApplication.ShareholderCertification = ShareholderCertification;
            #endregion

            if(requestDirector.RecordMembershipApplication(membershipApplication))
            {
                HttpContext.Session.SetString("success", "Membership application recorded successfully");
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, "Membership application could not be recorded");
            return Page();
        }

        public JsonResult OnPostCheckDate()
        {
            if ((DateTime.Now - DateOfBirth).Days < 0)
                return new JsonResult("Date of birth cannot be in the future");
            if ((DateTime.Now - Shareholder1SigningDate).Days < 0)
                return new JsonResult("Shareholder 1 signing date cannot be in the future");
            if ((DateTime.Now - Shareholder2SigningDate).Days < 0)
                return new JsonResult("Shareholder 2 signing date cannot be in the future");
            return new JsonResult(true);
        }
    }
}