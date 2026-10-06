using System.Globalization;

namespace CuraLink.MVC.Models.Doctors
{
    // Mirrors GET /api/doctors/patients/{patientId}.
    // StorageKey is intentionally NOT mapped, so it can never reach the Razor view.
    public class DoctorPatientProfileViewModel
    {
        public Guid PatientId { get; set; }
        public Guid PatientUserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string? BloodType { get; set; }

        public DoctorMedicalHistoryViewModel? MedicalHistory { get; set; }
        public List<DoctorMedicalDocumentViewModel> Documents { get; set; } = new();
    }

    public class DoctorMedicalHistoryViewModel
    {
        public Guid Id { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DoctorMedicalDocumentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }

        public bool IsPdf =>
            string.Equals(ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

        public bool IsImage =>
            ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        public string IconClass =>
            IsPdf ? "bi-file-earmark-pdf" : IsImage ? "bi-file-earmark-image" : "bi-file-earmark-text";

        public string FileTypeLabel
        {
            get
            {
                if (IsPdf) return "PDF";
                if (IsImage) return ContentType[6..].ToUpperInvariant().Replace("JPEG", "JPG");

                var ext = Path.GetExtension(FileName)?.TrimStart('.');
                return string.IsNullOrWhiteSpace(ext) ? "File" : ext.ToUpperInvariant();
            }
        }

        public string FormattedFileSize => FormatFileSize(FileSize);

        // Bytes / KB / MB / GB
        public static string FormatFileSize(long bytes)
        {
            string[] units = { "Bytes", "KB", "MB", "GB" };
            double size = bytes;
            var unit = 0;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return unit == 0
                ? $"{bytes} Bytes"
                : $"{size.ToString("0.#", CultureInfo.InvariantCulture)} {units[unit]}";
        }
    }

    // A document fetched from the API, held server-side and streamed to the browser by MVC.
    public sealed record DoctorMedicalDocumentFile(byte[] Content, string ContentType, string? FileName);
}