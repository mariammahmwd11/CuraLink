namespace CuraLink.MVC.Models.Prescriptions
{
    /// <summary>
    /// One row of GET /api/doctors/patients:
    /// { patientId, patientUserId, fullName, age, bloodType }
    /// </summary>
    public class DoctorPatientOptionViewModel
    {
        /// <summary>Patient.Id. Not used by the create-prescription contract.</summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Patient's ApplicationUserId — this is the value posted with the
        /// prescription (CreatePrescriptionCommand.PatientUserId).
        /// </summary>
        public Guid PatientUserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int Age { get; set; }

        public string? BloodType { get; set; }

        public string DisplayText
        {
            get
            {
                var extras = new List<string> { $"{Age} yrs" };

                if (!string.IsNullOrWhiteSpace(BloodType))
                {
                    extras.Add(BloodType!);
                }

                return $"{FullName} — {string.Join(", ", extras)}";
            }
        }
    }
}