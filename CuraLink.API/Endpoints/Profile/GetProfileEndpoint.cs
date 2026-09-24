using CuraLink.Application.Features.Profile.Queries.GetProfile;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Profile
{
    public static class GetProfileEndpoint
    {
        public static void MapGetProfileEndpoint(
            this WebApplication app)
        {
            app.MapGet(
                "/api/profile",
                async (
                    ClaimsPrincipal user,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = user.FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var query = new GetProfileQuery(userId);

                    var result = await sender.Send(
                        query,
                        cancellationToken);

                    return Results.Ok(result);
                })
            .RequireAuthorization();
        }
    }
}
