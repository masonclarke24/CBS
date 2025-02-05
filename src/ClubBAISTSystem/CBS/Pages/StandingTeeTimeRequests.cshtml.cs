﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using TechnicalServices;
using System.IO;

namespace CBS.Pages
{
    [Authorize(Roles = "Shareholder")]
    public class StandingTeeTimeRequestsModel : PageModel
    {
        private UserManager<ApplicationUser> userManager;
        public readonly ApplicationDbContext dbContext;

        public StandingTeeTimeRequestsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        [BindProperty, TempData, Display(Name = "Day of week")]
        public DayOfWeek DayOfWeek { get; set; }
        [BindProperty, Required, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [BindProperty, Required, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        public List<StandingTeeTime> STTRequests { get; set; }
        [BindProperty, Required, ArrayLength(3, ErrorMessage = "A standing tee time request requires exactly 3 additional members"), Distinct(ErrorMessage = "Supplied members must be unique")]
        public string[] SuppliedMemberNumbers { get; set; }
        public bool Confirmation { get; private set; }
        public bool ValidTimeSelected { get; private set; } = false;
        public DateTime InitialDate { get; set; }

        public void OnGet()
        {
            if (Request.Query.TryGetValue("selectedTime", out StringValues selectedTime)
                && (TempData.Peek<IEnumerable<string>>("PermissableTimes")?.Any(s => s == selectedTime) ?? false))
            {
                ValidTimeSelected = true;
                TempData["selectedTime"] = selectedTime.ToString();
            }
        }
        public void OnPost(){

        }
        public IActionResult OnPostSelect(string selectedTime)
        {

            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (requestDirector.FindStandingTeeTimeRequest(userManager.GetUserId(User)) != null)
                return Redirect("/MaxStandingTeeTimeRequest");

            Confirmation = false;
            TempData["selectedTime"] = selectedTime;
            return Redirect($"/StandingTeeTimeRequests?selectedTime={System.Web.HttpUtility.UrlEncode(selectedTime)}");
        }
        public void OnPostView(string startDate, string endDate)
        {
            if (DateTime.TryParse(startDate, out DateTime sd) && DateTime.TryParse(endDate, out DateTime ed))
            {
                StartDate = sd;
                if ((DateTime.Today.AddDays(8) - StartDate).Days > 0)
                    ErrorMessages.Add($"Start Date must be beyond { DateTime.Today.AddDays(7).ToLongDateString()}");
                if (ed.Ticks > 0)
                {
                    EndDate = ed;
                    if (StartDate.DayOfWeek != EndDate.DayOfWeek)
                        ErrorMessages.Add($"Day of week for Start Date ({StartDate.DayOfWeek.ToString()}) does not match day of week for End Date({EndDate.DayOfWeek.ToString()})");
                }
                else
                    ErrorMessages.Add("Please select an End Date");
                if (!(ErrorMessages?.Any() ?? false))
                {
                    TempData[nameof(StartDate)] = StartDate;
                    TempData[nameof(EndDate)] = EndDate;
                        Domain.CBS requestDirector = new Domain.CBS(userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id,
                            Startup.ConnectionString);
                    STTRequests = requestDirector.ViewStandingTeeTimeRequests(StartDate, EndDate);
                    TempData.Put("PermissableTimes", from time in STTRequests where time.Members is null select time.RequestedTime.ToString("HH:mm"));
                    return;
                }
            }
            TempData.Put(nameof(ErrorMessages), ErrorMessages);

        }

        public IActionResult OnPostSubmit()
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (requestDirector.FindStandingTeeTimeRequest(userManager.GetUserId(User)) != null)
                return Redirect("/MaxStandingTeeTimeRequest");

            ErrorMessages.Clear();
            bool isError = false;
            var errors = from error in ModelState
                         where (error.Key == nameof(SuppliedMemberNumbers) || error.Key == nameof(StartDate) || error.Key == nameof(EndDate))&&
                         error.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid && error.Value.Errors.Any()
                         select error.Value.Errors;
            if (errors.Any())
            {
                ErrorMessages.AddRange(from errorMessage in errors select errorMessage into messages from m in messages select m.ErrorMessage);
                isError = true;
            }

            var signedInMember = userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult();
            requestDirector = new Domain.CBS(signedInMember.Id, Startup.ConnectionString);

            StartDate = (DateTime)TempData.Peek(nameof(StartDate));
            EndDate = (DateTime)TempData.Peek(nameof(EndDate));
            StandingTeeTime requestedStandingTeeTime = new StandingTeeTime() { StartDate = StartDate, EndDate = EndDate, RequestedTime = DateTime.Parse(TempData.Peek("selectedTime").ToString()) };
            SuppliedMemberNumbers = SuppliedMemberNumbers.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            for (int i = 0; i < SuppliedMemberNumbers.Length; i++)
                SuppliedMemberNumbers[i] = SuppliedMemberNumbers[i].Trim();
            var userIds = (from user in dbContext.Users where SuppliedMemberNumbers.Contains(user.MemberNumber) select user.Id).ToList();

            if (userIds.Count() != SuppliedMemberNumbers.Count())
            {
                ErrorMessages.Add("One or more supplied member numbers do not exist");
                isError = true;
            }

            if (SuppliedMemberNumbers.Contains(signedInMember.MemberNumber))
            {
                ErrorMessages.Add("Do not enter your own member number");
                isError = true;
            }

            if (StartDate.DayOfWeek != EndDate.DayOfWeek)
            {
                ErrorMessages.Add($"Day of week for start date ({StartDate.DayOfWeek}) must match day of week for end date ({EndDate.DayOfWeek})");
                isError = true;
            }

            string currentUser = userManager.GetUserId(User);
            requestedStandingTeeTime.Members = userIds.Append(currentUser).ToList();
            requestedStandingTeeTime.SubmittedBy = userManager.GetUserId(User);

            if(isError)
            {
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"]);
            }

            if (!requestDirector.RequestStandingTeeTime(requestedStandingTeeTime, out string message))
            {
                ErrorMessages.Add(message);
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"]);
            }
            Confirmation = true;
            return Page();
        }
    }
}