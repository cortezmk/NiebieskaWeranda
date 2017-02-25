using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiebieskaWeranda.Attributes
{
    public class CheckDateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Date is empty");
            }
            var dt = (DateTime)value;
            return dt > DateTime.UtcNow
                ? ValidationResult.Success
                : new ValidationResult(string.IsNullOrEmpty(ErrorMessage)
                    ? "Make sure your date is >= than today"
                    : ErrorMessage);
        }

    }
}
