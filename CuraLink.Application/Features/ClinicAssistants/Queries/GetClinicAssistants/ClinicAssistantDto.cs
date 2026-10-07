namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistants;

public record ClinicAssistantDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime JoinedAt);