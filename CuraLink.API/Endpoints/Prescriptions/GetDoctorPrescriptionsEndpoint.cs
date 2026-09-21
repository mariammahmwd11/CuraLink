using CuraLink.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Prescriptions
{
    public static class GetDoctorPrescriptionsEndpoint
    {
        public static IEndpointRouteBuilder MapGetDoctorPrescriptionsEndpoint(
            this IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/prescriptions",
                async (
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = httpContext.User.FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await sender.Send(
                        new GetDoctorPrescriptionsQuery(userId),
                        cancellationToken);

                    return Results.Ok(result);
                })
                .RequireAuthorization("Doctor");

            return app;
        }
    }
}
