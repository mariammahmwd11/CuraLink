namespace CuraLink.MVC.Models.Home
{
    public class LandingViewModel
    {
        public string? DashboardController { get; set; }
        public string? DashboardAction { get; set; }

        public bool HasDashboard =>
            DashboardController != null && DashboardAction != null;
    }
}