using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Doctors
{
    public class DoctorReview
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; } 

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public Doctor Doctor { get; set; } = null!;
    }
}
