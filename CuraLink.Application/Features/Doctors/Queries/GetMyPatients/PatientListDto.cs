using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Doctors.Queries.GetMyPatients
{
    public record PatientListDto(
     Guid PatientId,
     string PatientUserId,
     int Age,
     string? BloodType
 );
}
