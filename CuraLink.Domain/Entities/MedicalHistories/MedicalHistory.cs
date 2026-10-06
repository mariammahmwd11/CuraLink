using CuraLink.Domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.MedicalHistories
{
    public class MedicalHistory
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Patient Patient { get; set; } = null!;

        public ICollection<MedicalDocument> Documents { get; set; }
            = new List<MedicalDocument>();
    }
}
