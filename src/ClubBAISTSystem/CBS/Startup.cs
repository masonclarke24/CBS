using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using CBS.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CBS
{
    public class Startup
    {
        public static string ConnectionString { get; private set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetConnectionString("CBSConnection");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("CBSConnection")));
            services.AddIdentity<ApplicationUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();

            services.AddSession();
            services.AddMemoryCache();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ManageMemberships", policy => policy.RequireRole("MembershipCommittee"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();
            app.UseSession();

            app.UseStatusCodePages();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //var membershipCommittee = new ApplicationUser() { Email = "memberships@cbg.ca", UserName = "memberships@cbg.ca" };
            //var newUser = userManager.CreateAsync(membershipCommittee, "Baist123$").GetAwaiter().GetResult();
            //roleManager.CreateAsync(new IdentityRole("MembershipCommittee")).GetAwaiter().GetResult();
            //userManager.AddToRoleAsync(membershipCommittee, "MembershipCommittee").GetAwaiter().GetResult();

            //var golfProfessional = new ApplicationUser() { Email = "golfProfessional@cbg.ca", UserName = "golfProfessional@cbg.ca" };
            //userManager.CreateAsync(golfProfessional, "Baist123$").GetAwaiter().GetResult();

            //roleManager.CreateAsync(new IdentityRole("ProShop")).GetAwaiter().GetResult();
            //userManager.AddToRoleAsync(userManager.FindByIdAsync("ffb114b9-a4ec-4aac-be5a-63e84e9d0719").GetAwaiter().GetResult(), "ProShop").GetAwaiter().GetResult();

            

            //var golfer = new ApplicationUser() { Email = "golfer4@test.com", UserName = "golfer4@test.com", PhoneNumber = "(780) 456 9335", MemberName = "Copper Member", MemberNumber = "4", MembershipLevel = "Bronze"};
            //userManager.CreateAsync(golfer, "Baist123$").GetAwaiter().GetResult();
            ////roleManager.CreateAsync(new IdentityRole("Shareholder")).GetAwaiter().GetResult();
            //userManager.AddToRoleAsync(golfer, "Golfer").GetAwaiter().GetResult();
            //userManager.AddToRoleAsync(golfer, "Shareholder").GetAwaiter().GetResult();
        }
    }
}
