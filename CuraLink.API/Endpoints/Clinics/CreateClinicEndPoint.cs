using CuraLink.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.DoctorEndpoints;

public static class CreateClinicEndpoint
{
    public static void MapCreateClinicEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/doctor/CreateClinic",
            async (
                CreateClinicCommand command,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                command.UserId = userId;

                var clinicId = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/doctor/clinics/{clinicId}",
                    new
                    {
                        id = clinicId,
                        message = "Clinic created successfully."
                    });
            })
            .RequireAuthorization();
    }
}