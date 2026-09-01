using CuraLink.Domain.Entities.Clinics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Doctors
{
    public class Doctor
    {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; } = null!;

        public string Specialty { get; set; } = null!;

        public string SyndicateId { get; set; } = null!;

        public DoctorStatusEnum Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? ChangeStatusByAdminId { get; set; }
       

        public ICollection<DoctorDocument> Documents { get; set; }
            = new List<DoctorDocument>();

        public ICollection<Clinic> Clinics { get; set; }
    = new List<Clinic>();
        public void Verify(Guid adminId)
        {
            Status = DoctorStatusEnum.verified;
            RejectionReason = null;
            VerifiedAt = DateTime.UtcNow;
            ChangeStatusByAdminId = adminId;
        }

        public void Reject(string reason, Guid adminId)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.");

            Status = DoctorStatusEnum.Rejected;
            RejectionReason = reason;
            VerifiedAt = null;
            ChangeStatusByAdminId = adminId;
        }
    }

}
