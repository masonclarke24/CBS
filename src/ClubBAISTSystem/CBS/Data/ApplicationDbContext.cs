using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CBS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.Entity<ApplicationUser>().Property("MemberNumber").HasColumnType("CHAR(10)").IsRequired(true);
            //builder.Entity<ApplicationUser>().Property("MemberName").HasColumnType("VARCHAR(40)").IsRequired(true);
            builder.Entity<ApplicationUser>().Property("MembershipLevel").HasColumnType("CHAR(10)").IsRequired(false);
        }
    }
}
