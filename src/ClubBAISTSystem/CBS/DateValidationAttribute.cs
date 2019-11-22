using System;
using System.ComponentModel.DataAnnotations;

namespace CBS.Pages
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (!(value is DateTime newDate))
            {
                ErrorMessage = "Param \"value\" is not type DateTime";
                return false;
            }

            if (newDate.Ticks == 0)
            {
                ErrorMessage = "Supplied date is invalid";
                return false;
            }

            if (newDate.Date == DateTime.Today.Date)
            {
                ErrorMessage = "Cannot reserve tee time for today";
                return false;
            }

            if ((DateTime.Today.AddDays(7) - newDate).TotalDays < 0)
            {
                ErrorMessage = $"Selected day must not be beyond {DateTime.Today.AddDays(7).ToLongDateString()}";
                return false;
            }

            if ((DateTime.Today - newDate).TotalDays > 0)
            {
                ErrorMessage = "Selected day cannot be in the past";
                return false;
            }

            return true;
        }
    }
}