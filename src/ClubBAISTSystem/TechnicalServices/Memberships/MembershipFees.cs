using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TechnicalServices.Memberships
{
    public class MembershipFees
    {
        public virtual DataTable FeeDetails { get; protected set; }
    }

    public class Shareholder : MembershipFees
    {
        private DataTable feeDetails;
        public Shareholder()
        {
            feeDetails = new DataTable(null);
            feeDetails.Columns.Add("Description");
            feeDetails.Columns.Add("Amount");
            feeDetails.Columns.Add("Due Date");

            feeDetails.Rows.Add("Share purchase price", 1000, DateTime.Today);
            feeDetails.Rows.Add("Entrance fee", 10000, DateTime.Today.AddYears(2));
            feeDetails.Rows.Add("Membership fee", 3000, new DateTime(DateTime.Today.Year, 4,1));
            feeDetails.Rows.Add("Food and beverage charge",500, DateTime.Today);
        }

        public override DataTable FeeDetails { get => feeDetails; protected set => base.FeeDetails = value; }

    }

    public class Associate : MembershipFees
    {
        private DataTable feeDetails;
        public Associate()
        {
            feeDetails = new DataTable(null);            
            feeDetails.Columns.Add("Description");
            feeDetails.Columns.Add("Amount");
            feeDetails.Columns.Add("Due Date");

            feeDetails.Rows.Add("Entrance fee", 10000, DateTime.Today.AddYears(2));
            feeDetails.Rows.Add("Membership fee", 4500, new DateTime(DateTime.Today.Year, 4, 1));
            feeDetails.Rows.Add("Food and beverage charge", 500, DateTime.Today);
        }

        public override DataTable FeeDetails { get => feeDetails; }

    }
}
