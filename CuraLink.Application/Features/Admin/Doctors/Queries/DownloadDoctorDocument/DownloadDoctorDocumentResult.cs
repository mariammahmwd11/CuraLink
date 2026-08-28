namespace CuraLink.Application.Features.Admin.Doctors.Queries.DownloadDoctorDocument
{
    public class DownloadDoctorDocumentResult
    {
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}