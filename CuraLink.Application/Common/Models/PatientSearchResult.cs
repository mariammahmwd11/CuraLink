namespace CuraLink.Application.Common.Models;

public record PatientSearchResult(
    string PatientId,
    string PatientUserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);