using CuraLink.Application.Features.Patients.Commands.UploadMedicalDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Patients
{
    public static class UploadMedicalDocumentEndpoint
    {
        public static void MapUploadMedicalDocumentEndpoint(
            this WebApplication app)
        {
            app.MapPost(
                "/api/patient/medical-documents",
                async (
                    [FromForm] UploadMedicalDocumentCommand command,
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

                    var documentId = await sender.Send(
                        command,
                        cancellationToken);

                    return Results.Created(
                        $"/api/patient/medical-documents/{documentId}",
                        new
                        {
                            id = documentId,
                            message = "Medical document uploaded successfully."
                        });
                })
                .RequireAuthorization()
                .DisableAntiforgery();
        }
    }
}
