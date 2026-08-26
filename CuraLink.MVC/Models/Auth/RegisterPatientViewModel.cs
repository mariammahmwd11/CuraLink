using System.ComponentModel.DataAnnotations;
using CuraLink.MVC.Validation;

namespace CuraLink.MVC.Models.Auth
{
    public class RegisterPatientViewModel
    {
            [Required(ErrorMessage = "First name is required.")]
            [StringLength(50, ErrorMessage = "First name can't exceed 50 characters.")]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required.")]
            [StringLength(50, ErrorMessage = "Last name can't exceed 50 characters.")]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Phone number is required.")]
            [Phone(ErrorMessage = "Please enter a valid phone number.")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
            [RegularExpression(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
                ErrorMessage = "Password must include upper & lower case letters, a number, and a special character.")]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Please confirm your password.")]
            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            [Display(Name = "Confirm password")]
            public string ConfirmPassword { get; set; }

            [RequiredTrue(ErrorMessage = "You must accept the Terms & Conditions to continue.")]
            [Display(Name = "I agree to the Terms & Conditions and Privacy Policy")]
            public bool AcceptTerms { get; set; }
        }
    }

