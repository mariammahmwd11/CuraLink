using System;

namespace CuraLink.MVC.Models.Clinics
{
    /// <summary>
    /// Read-model for a single clinic card on the Doctor Dashboard / My Clinics section.
    /// Populated from GET /api/doctor/clinics via ClinicApiClient.GetMyClinicsAsync().
    /// </summary>
    public class ClinicSummaryViewModel
    {
        public Guid Id { get; set; }

        public string ClinicName { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public decimal ConsultationPrice { get; set; }

        // Nullable: only rendered on the card if the API actually returns it.
        public int? StaffCount { get; set; }

        // Defaults to "Active" until the backend contract confirms real status values
        // (e.g. Active / PendingVerification / Inactive).
        public string Status { get; set; } = "Active";
    }
}
