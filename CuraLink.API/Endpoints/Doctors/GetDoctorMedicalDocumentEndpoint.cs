using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Doctors;

public static class GetDoctorMedicalDocumentEndpoint
{
    public static void MapGetDoctorMedicalDocumentEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/doctors/patients/{patientId:guid}/documents/{documentId:int}",
            async (
                Guid patientId,
                int documentId,
                ClaimsPrincipal user,
                IUnitOfWork unitOfWork,
                IMedicalDocumentRepository medicalDocumentRepository,
                IFileStorageService fileStorageService,
                CancellationToken cancellationToken) =>
            {
                var doctorUserId =
                    user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(doctorUserId))
                {
                    return Results.Unauthorized();
                }

                // Get current doctor
                var doctor = await unitOfWork.Doctors
                    .GetByApplicationUserIdAsync(
                        doctorUserId,
                        cancellationToken);

                if (doctor is null)
                {
                    return Results.NotFound(new
                    {
                        message = "Doctor not found."
                    });
                }

                // Verify that this patient belongs to this doctor
                var doctorPatient =
                    await unitOfWork.DoctorPatients.GetAsync(
                        doctor.Id,
                        patientId,
                        cancellationToken);

                if (doctorPatient is null)
                {
                    return Results.Forbid();
                }

                // Get document belonging to this patient
                var document =
                    await medicalDocumentRepository
                        .GetByIdForPatientAsync(
                            documentId,
                            patientId,
                            cancellationToken);

                if (document is null)
                {
                    return Results.NotFound(new
                    {
                        message = "Medical document not found."
                    });
                }

                var stream =
                    await fileStorageService.DownloadAsync(
                        document.StorageKey,
                        cancellationToken);

                return Results.File(
                    stream,
                    document.ContentType);
            })
        .RequireAuthorization(policy =>
            policy.RequireRole("Doctor"))
        .DisableAntiforgery()
        .WithTags("Doctors");
    }
}