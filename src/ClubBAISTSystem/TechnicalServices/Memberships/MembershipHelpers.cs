﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace TechnicalServices
{
    public enum ApplicationStatus
    {
        New,
        Accepted,
        Denied,
        OnHold,
        Waitlisted
    }

    public enum MembershipType
    {
        Shareholder,
        Associate
    }

    public struct ContactInformation
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string PrimaryPhone { get; set; }
        public string AlternatePhone { get; set; }
        public string Email { get; set; }
    }

}
