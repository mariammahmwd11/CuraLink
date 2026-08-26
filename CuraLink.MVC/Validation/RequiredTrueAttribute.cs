using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Validation
{
    /// <summary>
    /// Validates that a boolean property is true.
    /// Works with both server-side and client-side (jQuery Unobtrusive) validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredTrueAttribute : ValidationAttribute
    {
        public RequiredTrueAttribute()
        {
            ErrorMessage = "You must accept the Terms & Conditions to continue.";
        }

        public override bool IsValid(object value)
        {
            // Allow null for reference types, but enforce true for boolean
            if (value == null)
                return false;

            if (value is bool boolValue)
                return boolValue;

            return false;
        }
    }
}
