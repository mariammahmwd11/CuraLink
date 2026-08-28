namespace CuraLink.MVC.Models.Admin
{
    /// <summary>
    /// Mirrors CuraLink.Application.Features.Admin.Doctors.Queries.GetDoctorDocument.DoctorDocumentResult
    /// returned by GET /api/admin/doctors/{doctorId}/documents.
    /// </summary>
    public class DoctordocumentViewModel
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Raw storage URL returned by the backend. This URL points at an
        /// authenticated Cloudinary resource and is NOT directly browsable,
        /// so the UI never links to it directly - it is only kept here for
        /// completeness/debugging. "View Document" instead calls the MVC
        /// proxy action (AdminController.DownloadDocument), which forwards
        /// the admin's bearer token to the existing backend download
        /// endpoint (GET /api/admin/doctors/{doctorId}/documents/{documentId}/download).
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
