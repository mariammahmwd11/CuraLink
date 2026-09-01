using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.MedicalHistories
{
    public class MedicalDocument
    {
        public int Id { get; set; }

        public Guid MedicalHistoryId { get; set; }

        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public string StorageKey { get; set; } = null!;

        public DateTime UploadedAt { get; set; }

        public MedicalHistory MedicalHistory { get; set; } = null!;
    }
}
