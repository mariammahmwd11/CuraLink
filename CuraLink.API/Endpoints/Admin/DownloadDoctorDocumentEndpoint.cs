using CuraLink.Application.Features.Admin.Doctors.Queries.DownloadDoctorDocument;
using MediatR;

namespace CuraLink.API.Endpoints.Admin
{
    public static class DownloadDoctorDocumentEndpoint
    {
        public static void MapDownloadDoctorDocumentEndpoint(
            this WebApplication app)
        {
            app.MapGet(
                "/api/admin/doctors/{doctorId:guid}/documents/{documentId:int}/download",
                async (
                    Guid doctorId,
                    int documentId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new DownloadDoctorDocumentQuery(
                            doctorId,
                            documentId),
                        cancellationToken);

                    return Results.File(
                        result.FileStream,
                        result.ContentType,
                        result.FileName);
                })
                .RequireAuthorization("AdminOnly")
                .WithName("DownloadDoctorDocument")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .WithTags("Admin");
        }
    }
}