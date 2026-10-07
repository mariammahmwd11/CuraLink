using CuraLink.Application.Features.Admin.Doctors.Queries.GetDoctorDocument;
using MediatR;

namespace CuraLink.API.Endpoints.Admin
{
    public static class GetDoctorDocumentEndpoint
    {
        public static void MapGetDoctorDocumentEndpoint(
            this WebApplication app)
        {
            app.MapGet("/api/admin/doctors/{doctorId:guid}/documents",
             async (Guid doctorId, ISender sender, CancellationToken cancellationToken) =>
             {
                 var result = await sender.Send(
                     new GetDoctorDocumentQuery(doctorId),
                     cancellationToken);

                 return Results.Ok(result);
             })
             .RequireAuthorization("AdminOnly")
             .WithName("GetDoctorDocuments")
             .Produces<List<DoctorDocumentResult>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound)
             .WithTags("Admin");
        }
    }
}
