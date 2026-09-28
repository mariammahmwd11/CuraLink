using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Payments
{
    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Refunded = 3
    }
}
