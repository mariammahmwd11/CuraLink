using CuraLink.Application.Features.Doctors.Queries.GetMyPatients;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Doctors
{
    public static class GetMyPatientsEndpoint
    {
        public static void MapGetMyPatientsEndpoint(
            this WebApplication app)
        {
            app.MapGet(
                "/api/doctors/patients",
                async (
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = httpContext.User
                        .FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var patients = await sender.Send(
                        new GetMyPatientsQuery(userId),
                        cancellationToken);

                    return Results.Ok(patients);
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Doctor"))
                .WithTags("Doctors");
        }
    }
}
