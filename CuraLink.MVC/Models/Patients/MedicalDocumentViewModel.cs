using System;

namespace CuraLink.MVC.Models.Patients
{
  
    public class MedicalDocumentViewModel
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        public string FileSizeDisplay =>
            FileSize >= 1024 * 1024
                ? $"{FileSize / (1024.0 * 1024.0):0.##} MB"
                : $"{Math.Max(FileSize / 1024.0, 0.1):0.#} KB";

        public string FileIconClass => ContentType switch
        {
            "application/pdf" => "bi-file-earmark-pdf-fill",
            "image/jpeg" or "image/jpg" => "bi-file-earmark-image-fill",
            "image/png" => "bi-file-earmark-image-fill",
            _ => "bi-file-earmark-fill"
        };
    }
}