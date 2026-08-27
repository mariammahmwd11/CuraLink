using CuraLink.Application.Features.Authentication.Commands.RegisterDoctor;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.API.Endpoints.AuthEndPoints;

public static class RegisterDoctorEndpoint
{
    public static void MapRegisterDoctorEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/auth/register-doctor",
            async (
               [FromForm] RegisterDoctorCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(command, cancellationToken);

                return Results.Accepted();
            })
            .DisableAntiforgery();
    }
}