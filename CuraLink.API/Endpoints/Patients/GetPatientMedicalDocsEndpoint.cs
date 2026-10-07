using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.PatientEndPoints;

public static class GetPatientMedicalDocsEndpoint
{
    public static void MapGetPatientMedicalDocsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/patient/View-medical-records",
            async (
                ClaimsPrincipal user,
                IPatientRepository patientRepository,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                // Get ApplicationUserId from JWT claims
                var userId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                // Get Patient using ApplicationUserId
                var patient = await patientRepository
                    .GetByApplicationUserIdAsync(
                        userId,
                        cancellationToken);

                if (patient is null)
                {
                    return Results.NotFound(new
                    {
                        message = "Patient not found."
                    });
                }

                // Get patient's medical documents
                var documents = await sender.Send(
                    new GetPatientMedicalDocsQuery(patient.Id),
                    cancellationToken);

                return Results.Ok(documents);
            })
        .RequireAuthorization().WithTags("Patients");
    }
}