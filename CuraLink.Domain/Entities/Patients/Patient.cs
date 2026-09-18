using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Domain.Entities.Prescriptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Patients
{
    public class Patient
    {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string? BloodType { get; set; }

        public MedicalHistory? MedicalHistory { get; set; }
        public ICollection<DoctorPatient> Doctors { get; set; }
    = new List<DoctorPatient>();

        public ICollection<Prescription> Prescriptions { get; set; }
            = new List<Prescription>();
    }
}
