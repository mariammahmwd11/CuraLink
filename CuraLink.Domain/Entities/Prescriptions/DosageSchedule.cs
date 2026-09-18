using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Prescriptions
{
    public class DosageSchedule
    {
        public Guid Id { get; set; }

        public Guid PrescriptionItemId { get; set; }

        public TimeSpan DosageTime { get; set; }

        public PrescriptionItem PrescriptionItem { get; set; } = null!;
    }
}
