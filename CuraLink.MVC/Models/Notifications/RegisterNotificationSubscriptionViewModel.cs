namespace CuraLink.MVC.Models.Notifications
{
    public class RegisterNotificationSubscriptionViewModel
    {
        public string Endpoint { get; set; } = null!;
        public string P256DH { get; set; } = null!;
        public string Auth { get; set; } = null!;
    }
}
