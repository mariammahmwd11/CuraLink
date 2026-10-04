using CuraLink.Domain.Entities.Clinics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.ClinicAssistants
{
    public class ClinicAssistant
    {
        public Guid Id { get; set; }

        public Guid ClinicId { get; set; }

        public string ApplicationUserId { get; set; } = null!;

        public DateTime JoinedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public Clinic Clinic { get; set; } = null!;
    }
}
