using CuraLink.Domain.Entities.Appointments;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.ChatMessages
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public string SenderId { get; set; } = null!;

        public string ReceiverId { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public Appointment Appointment { get; set; } = null!;
    }
}
