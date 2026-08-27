using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Domain.Entities.Doctor
{
    public enum DoctorStatusEnum
    {
        PendingVerification = 1,
        Approved = 2,
        Rejected = 3
    }
}
