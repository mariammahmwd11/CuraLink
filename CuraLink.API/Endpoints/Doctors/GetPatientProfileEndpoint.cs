using CuraLink.Application.Features.Doctors.Queries.GetPatientProfile;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Doctors;

public static class GetPatientProfileEndpoint
{
    public static void MapGetPatientProfileEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/doctors/patients/{patientId:guid}",
            async (
                Guid patientId,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var doctorUserId =
                    user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(doctorUserId))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var result = await sender.Send(
                        new GetPatientProfileQuery(
                            doctorUserId,
                            patientId),
                        cancellationToken);

                    return Results.Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        message = ex.Message
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Forbid();
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new
                    {
                        message = ex.Message
                    });
                }
            })
        .RequireAuthorization(policy =>
            policy.RequireRole("Doctor"))
        .WithTags("Doctors");
    }
}