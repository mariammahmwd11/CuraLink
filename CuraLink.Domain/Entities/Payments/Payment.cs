using CuraLink.Domain.Entities.Appointments;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Payments
{
    public class Payment
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public PaymentStatus Status { get; set; }

        public string? TransactionId { get; set; }

        public string? StripeSessionId { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public Appointment Appointment { get; set; } = null!;
    }
}
