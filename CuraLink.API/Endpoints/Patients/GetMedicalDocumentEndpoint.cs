using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Features.Patients.Queries.GetMedicalDocument;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Patients;

public static class GetMedicalDocumentEndpoint
{
    public static void MapGetMedicalDocumentEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/patient/medical-documents/{id:int}",
            async (
                int id,
                ClaimsPrincipal user,
                ISender sender,
                IFileStorageService fileStorageService,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var document = await sender.Send(
                    new GetMedicalDocumentQuery
                    {
                        UserId = userId,
                        DocumentId = id
                    },
                    cancellationToken);

                if (document == null)
                {
                    return Results.NotFound(
                        new
                        {
                            message = "Medical document not found."
                        });
                }

                var stream = await fileStorageService.DownloadAsync(
                    document.StorageKey,
                    cancellationToken);

                return Results.File(
                    stream,
                    document.ContentType);
            })
            .RequireAuthorization()
            .DisableAntiforgery().WithTags("Patients");
    }
}