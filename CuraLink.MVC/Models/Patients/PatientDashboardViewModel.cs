namespace CuraLink.MVC.Models.Patients
{
    public class PatientDashboardViewModel
    {
        public PatientProfileViewModel Profile { get; set; } = new();

        public int MedicalDocumentsCount { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
    }
}