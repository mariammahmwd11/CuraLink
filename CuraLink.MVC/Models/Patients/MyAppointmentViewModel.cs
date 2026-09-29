namespace CuraLink.MVC.Models.Patients
{
    public class MyAppointmentViewModel
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? ClinicName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
    }
}
