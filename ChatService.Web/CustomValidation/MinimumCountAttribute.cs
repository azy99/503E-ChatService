using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatService.Web.ValidationAttributes
{
    public class MinimumCountAttribute : ValidationAttribute
    {
        private readonly int _minimumCount;

        public MinimumCountAttribute(int minimumCount)
        {
            _minimumCount = minimumCount;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not ICollection collection || collection.Count < _minimumCount)
            {
                return new ValidationResult($"The '{validationContext.DisplayName}' field must have at least {_minimumCount} items.");
            }

            return ValidationResult.Success;
        }
    }
}