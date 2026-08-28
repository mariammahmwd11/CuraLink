using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.GetDoctorDocument
{
    public class DoctorDocumentResult
    {
        public int Id { get; set; }
            public string FileName { get; set; } = null!;
            public string ContentType { get; set; } = null!;
            public long FileSize { get; set; }
            public DateTime UploadedAt { get; set; }
            public string Url { get; set; } = null!;
    }
    
}
