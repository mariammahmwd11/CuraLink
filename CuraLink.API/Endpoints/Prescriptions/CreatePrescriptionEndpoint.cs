using CuraLink.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Prescriptions
{
    public static class CreatePrescriptionEndpoint
    {
        public static void MapCreatePrescriptionEndpoint(
            this WebApplication app)
        {
            app.MapPost(
                "/api/prescriptions",
                async (
                    [FromBody] CreatePrescriptionRequest request,
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = httpContext.User.FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var command = new CreatePrescriptionCommand(
                        userId,
                        request.PatientUserId,
                        request.StartDate,
                        request.EndDate,
                        request.Items);

                    var prescriptionId = await sender.Send(
                        command,
                        cancellationToken);

                    return Results.Ok(new
                    {
                        Message = "Prescription created successfully.",
                        PrescriptionId = prescriptionId
                    });
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Doctor"))
                .WithTags("Prescriptions");
        }
    }
}