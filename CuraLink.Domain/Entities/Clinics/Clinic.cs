using CuraLink.Domain.Entities.Doctors;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Clinics
{
    public class Clinic
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }

        public string ClinicName { get; set; } = null!;

        public string Address { get; set; } = null!;

        public decimal ConsultationPrice { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}
