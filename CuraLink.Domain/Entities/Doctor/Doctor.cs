using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Doctor
{
    public class Doctor
    {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; } = null!;

        public string Specialty { get; set; } = null!;

        public string SyndicateId { get; set; } = null!;

        public DoctorStatusEnum Status { get; set; }
        public string? RejectionReason { get; set; }
        public ICollection<DoctorDocument> Documents { get; set; }
            = new List<DoctorDocument>();
    }

}
