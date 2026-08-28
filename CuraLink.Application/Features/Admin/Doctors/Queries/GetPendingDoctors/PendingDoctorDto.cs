using CuraLink.Domain.Entities.Doctor;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.GetPendingDoctors
{
    public class PendingDoctorDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Specialty { get; set; } = null!;

        public string SyndicateId { get; set; } = null!;

        public DoctorStatusEnum Status { get; set; }

       
    }
}
