using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Doctors
{
    public class DoctorDocument
    {
        public int Id { get; set; }

        public Guid DoctorId { get; set; }

        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public string StorageKey { get; set; } = null!;

        public DateTime UploadedAt { get; set; }

        public Doctor Doctor { get; set; } = null!;
    }
}
