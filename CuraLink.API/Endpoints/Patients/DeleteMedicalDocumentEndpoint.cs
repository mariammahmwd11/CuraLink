using CuraLink.Application.Features.Patients.Commands.DeleteMedicalDocument;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Patients;

public static class DeleteMedicalDocumentEndpoint
{
    public static void MapDeleteMedicalDocumentEndpoint(
        this WebApplication app)
    {
        app.MapDelete(
            "/api/patient/medical-documents/{id:int}",
            async (
                int id,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var deleted = await sender.Send(
                    new DeleteMedicalDocumentCommand
                    {
                        UserId = userId,
                        DocumentId = id
                    },
                    cancellationToken);

                if (!deleted)
                {
                    return Results.NotFound(
                        new
                        {
                            message = "Medical document not found."
                        });
                }

                return Results.Ok(
                    new
                    {
                        message = "Medical document deleted successfully."
                    });
            })
            .RequireAuthorization()
            .DisableAntiforgery();
    }
}