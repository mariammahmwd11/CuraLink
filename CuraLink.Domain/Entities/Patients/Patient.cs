using CuraLink.Domain.Entities.MedicalHistories;
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
    }
}
