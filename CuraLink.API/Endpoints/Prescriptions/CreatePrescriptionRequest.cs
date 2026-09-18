using CuraLink.Application.Features.Prescriptions.Commands.CreatePrescription;

namespace CuraLink.API.Endpoints.Prescriptions
{
    public record CreatePrescriptionRequest(
     string PatientUserId,
     DateTime StartDate,
     DateTime EndDate,
     List<PrescriptionItemRequest> Items
 );
}
