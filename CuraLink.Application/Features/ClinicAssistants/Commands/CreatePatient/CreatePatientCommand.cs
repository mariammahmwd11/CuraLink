using MediatR;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.CreatePatient;

public record CreatePatientCommand(
    string ApplicationUserId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string? BloodType,
    string? MedicalHistoryNotes
) : IRequest<Guid>;