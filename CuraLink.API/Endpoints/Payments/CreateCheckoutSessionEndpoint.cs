using System.Security.Claims;
using CuraLink.Application.Features.Payments.Commands.CreateCheckoutSession;
using MediatR;

namespace CuraLink.API.Endpoints.Payments;

public static class CreateCheckoutSessionEndpoint
{
    public static void MapCreateCheckoutSessionEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/payments/{appointmentId:int}/checkout",
            async (
                int appointmentId,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                var command =
                    new CreateCheckoutSessionCommand(
                        appointmentId,
                        userId);

                var checkoutUrl =
                    await sender.Send(
                        command,
                        cancellationToken);

                return Results.Ok(new
                {
                    checkoutUrl
                });
            })
            .RequireAuthorization().WithTags("Payments");
    }
}