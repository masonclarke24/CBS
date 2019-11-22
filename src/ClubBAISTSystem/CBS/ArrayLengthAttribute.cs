using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CBS.Pages
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class ArrayLengthAttribute : ValidationAttribute
    {
        private int requiredLength;

        public ArrayLengthAttribute(int requiredLength)
        {
            this.requiredLength = requiredLength;
        }

        public override bool IsValid(object value)
        {
            if (!(value is IEnumerable<object> ts)) return false;
            if(ts.Count(obj => !(obj is null)) != requiredLength)
                return false;
            return true;
        }
    }
}