using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Doctors;

public static class DownloadDoctorMedicalDocumentEndpoint
{
    public static void MapDownloadDoctorMedicalDocumentEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/doctors/patients/{patientId:guid}/documents/{documentId:int}/download",
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

                // Verify doctor-patient relationship
                var doctorPatient =
                    await unitOfWork.DoctorPatients.GetAsync(
                        doctor.Id,
                        patientId,
                        cancellationToken);

                if (doctorPatient is null)
                {
                    return Results.Forbid();
                }

                // Get patient's document
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
                    document.ContentType,
                    document.FileName);
            })
        .RequireAuthorization(policy =>
            policy.RequireRole("Doctor"))
        .DisableAntiforgery()
        .WithTags("Doctors");
    }
}