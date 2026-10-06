using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Models
{
    public record PatientSearchResult(
    string PatientId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);
}
