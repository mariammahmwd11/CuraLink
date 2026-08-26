using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

       
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
