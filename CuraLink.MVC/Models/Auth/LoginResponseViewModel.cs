namespace CuraLink.MVC.Models.Auth
{
    public class LoginResponseViewModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserProfileViewModel User { get; set; } = new();
    }
}
