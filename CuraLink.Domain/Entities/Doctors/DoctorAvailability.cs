using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Doctors
{
    public class DoctorAvailability
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }

        public DateTime AvailableDate { get; set; }

        public Doctor Doctor { get; set; } = null!;
    }
}
