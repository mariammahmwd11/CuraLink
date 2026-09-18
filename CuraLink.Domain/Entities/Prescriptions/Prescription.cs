using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Prescriptions
{
    public class Prescription
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public Guid DoctorId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public Patient Patient { get; set; } = null!;

        public Doctor Doctor { get; set; } = null!;

        public ICollection<PrescriptionItem> Items { get; set; }
            = new List<PrescriptionItem>();
    }
}
