using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CBS.Pages
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DistinctAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (!(value is IEnumerable<object> vector)) return false;

            int initialCount = vector.Count();
            if(initialCount != vector.Distinct().Count()) return false;
            return true;
        }
    }
}