using CuraLink.Application.Features.Authentication.Commands.RegisterPatient;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.API.Endpoints.AuthEndPoints;

public static class RegisterPatientEndPoint
{
    public static IEndpointRouteBuilder MapRegisterPatientEndpoint(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register-patient", async (
            RegisterPatientCommand command,
           [FromServices] ISender sender) =>
        {
            await sender.Send(command);

            return Results.Created(
                "/api/auth/register-patient",
                new
                {
                    Message = "Registration successful."
                });
        });

        return app;
    }
}