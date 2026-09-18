using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Prescriptions
{
    public class PrescriptionItem
    {
        public Guid Id { get; set; }

        public Guid PrescriptionId { get; set; }

        public string MedicationName { get; set; } = null!;

        public string Dosage { get; set; } = null!;

        public string? Instructions { get; set; }

        public Prescription Prescription { get; set; } = null!;

        public ICollection<DosageSchedule> Schedules { get; set; }
            = new List<DosageSchedule>();
    }
}
