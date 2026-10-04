using CuraLink.Domain.Entities.Clinics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.ClinicAssistants
{
    public class ClinicAssistantInvitation
    {
        public Guid Id { get; set; }

        public Guid ClinicId { get; set; }

        public string Email { get; set; } = null!;

        public string Token { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public Clinic Clinic { get; set; } = null!;
    }
}
