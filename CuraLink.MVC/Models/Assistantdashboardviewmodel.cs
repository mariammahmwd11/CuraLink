namespace CuraLink.MVC.Models
{
    public class AssistantDashboardViewModel
    {
        public string ClinicName { get; set; } = "";

        // Today's numbers
        public int TotalToday { get; set; }
        public int Waiting { get; set; }
        public int CheckedIn { get; set; }
        public int Completed { get; set; }

        public List<AssistantAppointmentRow> TodayAppointments { get; set; } = new();
    }

    public class AssistantAppointmentRow
    {
        public int AppointmentId { get; set; }
        public string Time { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        // Scheduled | CheckedIn | Completed | Cancelled
        public string Status { get; set; } = "Scheduled";
    }
}