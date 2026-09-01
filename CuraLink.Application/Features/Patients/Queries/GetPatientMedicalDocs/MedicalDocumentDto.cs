using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs
{
    public class MedicalDocumentDto
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string StorageKey { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}
