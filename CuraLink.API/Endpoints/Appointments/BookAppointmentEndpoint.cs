
using System.Security.Claims;
using CuraLink.Application.Features.Appointments.Commands.BookAppointment;
using MediatR;

namespace CuraLink.API.Endpoints.Appointments;

public static class BookAppointmentEndpoint
{
    public static void MapBookAppointmentEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/appointments",
            async (
                BookAppointmentRequest request,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var appointmentId = await sender.Send(
                    new BookAppointmentCommand(
                        request.DoctorId,
                        request.Date,
                        request.StartTime,
                        request.EndTime,
                        applicationUserId),
                    cancellationToken);

                return Results.Ok(new
                {
                    appointmentId
                });
            })
        .RequireAuthorization();
    }
}

