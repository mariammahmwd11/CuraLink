using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Appointments
{
    public enum AppointmentStatus
    {
        Pending,
        Paid,
        Confirmed,
        Cancelled,
        Completed
    }
}
