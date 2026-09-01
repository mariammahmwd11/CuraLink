using System.Collections.Generic;
using CuraLink.MVC.Models.Clinics;

namespace CuraLink.MVC.Models.Doctors
{
    public class DoctorDashboardViewModel
    {
        public string DoctorDisplayName { get; set; } = "Doctor";

     
        public int TotalClinics { get; set; }
        public int TodaysAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int TotalPatients { get; set; }

        public List<ClinicSummaryViewModel> Clinics { get; set; } = new();

        public bool HasClinics => Clinics.Count > 0;
    }
}