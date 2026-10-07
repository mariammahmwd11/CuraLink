using System.Security.Claims;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Doctors.Queries.GetAvailableDoctorSlots;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class GetAvailableClinicAssistantSlotsEndpoint
{
    public static void MapGetAvailableClinicAssistantSlotsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/clinic-assistants/available-slots",
            async (
                DateTime date,
                ClaimsPrincipal user,
                ISender sender,
                IApplicationDbContext context,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var assistant =
                    await context.ClinicAssistants
                        .FirstOrDefaultAsync(
                            x =>
                                x.ApplicationUserId == applicationUserId &&
                                x.IsActive,
                            cancellationToken);

                if (assistant is null)
                    return Results.NotFound(new
                    {
                        message = "Clinic assistant not found."
                    });

                var clinic =
                    await context.Clinics
                        .FirstOrDefaultAsync(
                            x => x.Id == assistant.ClinicId,
                            cancellationToken);

                if (clinic is null)
                    return Results.NotFound(new
                    {
                        message = "Clinic not found."
                    });

                var slots = await sender.Send(
    new GetAvailableDoctorSlotsQuery(
        clinic.DoctorId,
        date),
    cancellationToken);

                return Results.Ok(new
                {
                    slots
                });
            })
        .RequireAuthorization("Receptionist").WithTags("Clinic Assistants");
    }
}