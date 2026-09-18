using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities
{
    public class DoctorPatient
    {
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }

        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
