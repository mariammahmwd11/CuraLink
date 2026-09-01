using System.Collections.Generic;
using CuraLink.MVC.Models.Clinics;

namespace CuraLink.MVC.Models.Doctors
{
    public class DoctorDashboardViewModel
    {
        public string DoctorDisplayName { get; set; } = "Doctor";

        // TODO: wire these to real endpoints once Appointments/Patients APIs
        // are available for the doctor role. Left at 0 rather than faked data.
        public int TotalClinics { get; set; }
        public int TodaysAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int TotalPatients { get; set; }

        public List<ClinicSummaryViewModel> Clinics { get; set; } = new();

        public bool HasClinics => Clinics.Count > 0;
    }
}