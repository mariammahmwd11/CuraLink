namespace CuraLink.MVC.Models.Doctors
{
    // One row per distinct patient who has booked with the doctor.
    public class DoctorPatientViewModel
    {
        public string PatientName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public DateTime? LastAppointment { get; set; }   // most recent past appointment
        public DateTime? NextAppointment { get; set; }   // soonest future appointment
    }
}