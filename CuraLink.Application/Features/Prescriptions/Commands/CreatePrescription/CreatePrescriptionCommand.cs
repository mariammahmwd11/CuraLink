using MediatR;

namespace CuraLink.Application.Features.Prescriptions.Commands.CreatePrescription;

public record CreatePrescriptionCommand(
    string UserId,
    string PatientUserId,
    DateTime StartDate,
    DateTime EndDate,
    List<PrescriptionItemRequest> Items
) : IRequest<Guid>;

public record PrescriptionItemRequest(
    string MedicationName,
    string Dosage,
    string? Instructions,
    List<TimeSpan> DosageTimes
);