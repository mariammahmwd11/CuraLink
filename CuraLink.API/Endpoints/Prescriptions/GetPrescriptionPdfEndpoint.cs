
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Common.Interfaces.Prescriptions;
using CuraLink.Domain.Entities.Doctors;
using Microsoft.AspNetCore.Authorization;

namespace CuraLink.API.Endpoints.Prescriptions;

public static class GetPrescriptionPdfEndpoint
{
    public static IEndpointRouteBuilder MapGetPrescriptionPdfEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/prescriptions/{prescriptionId:guid}/pdf",
            async (
                Guid prescriptionId,
                HttpContext httpContext,
                IUnitOfWork unitOfWork,
                IPrescriptionPdfService pdfService,
                CancellationToken cancellationToken) =>
            {
                var userId =
                    httpContext.User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var doctor = await unitOfWork.Doctors
                    .GetByApplicationUserIdAsync(
                        userId,
                        cancellationToken);

                if (doctor == null)
                {
                    return Results.NotFound(
                        new
                        {
                            message = "Doctor not found."
                        });
                }

                if (doctor.Status != DoctorStatusEnum.verified)
                {
                    return Results.Forbid();
                }

                var prescription =
                    await unitOfWork.Prescriptions
                        .GetByIdWithDetailsAsync(
                            prescriptionId,
                            cancellationToken);

                if (prescription == null)
                {
                    return Results.NotFound(
                        new
                        {
                            message = "Prescription not found."
                        });
                }

                if (prescription.DoctorId != doctor.Id)
                {
                    return Results.Forbid();
                }

                var pdf = await pdfService.GenerateAsync(
                    prescriptionId,
                    cancellationToken);

                return Results.File(
                    pdf,
                    "application/pdf",
                    $"Prescription-{prescriptionId}.pdf");
            })
            .RequireAuthorization("Doctor").WithTags("Prescriptions");

        return app;
    }
}

