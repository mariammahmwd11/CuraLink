
using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Features.Authentication.DTOs;

namespace CuraLink.API.Endpoints.AuthEndPoints
{
    public static class LoginEndPoint
    {
        public static IEndpointRouteBuilder MapLoginEndPoint(
            this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/login", async (
                LoginRequestDTO request,
                IAuthService authService) =>
            {
                var result = await authService.LoginAsync(request);

                return Results.Ok(result);
            });

            return app;
        }
    }
}

