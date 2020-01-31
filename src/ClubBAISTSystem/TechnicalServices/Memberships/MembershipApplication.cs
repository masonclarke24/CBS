using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalServices.Memberships
{
    public class MembershipApplication
    {
        public ContactInformation ProspectiveMemberContactInfo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public MembershipType MembershipType { get; set; }
        public string Occupation { get; set; }
        public ContactInformation EmploymentDetails { get; set; }
        public bool ProspectiveMemberCertification { get; set; }
        public DateTime? ApplicationDate => applicationDate;
        public string Shareholder1 { get; set; }
        public string Shareholder2 { get; set; }
        public DateTime Shareholder1SigningDate { get; set; }
        public DateTime Shareholder2SigningDate { get; set; }
        public bool ShareholderCertification { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        private readonly DateTime? applicationDate;

        public MembershipApplication(DateTime applicationDate)
        {
            this.applicationDate = applicationDate;
        }

        public MembershipApplication()
        {

        }
    }
}
