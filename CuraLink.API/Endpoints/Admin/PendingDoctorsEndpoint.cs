using CuraLink.Application.Features.Admin.Doctors.Queries.GetPendingDoctors;
using CuraLink.Application.Features.Admin.Doctors.Queries.GetPendingDoctors;
using MediatR;

namespace CuraLink.API.Endpoints.Admin
{
    public static class PendingDoctorsEndpoint
    {
        public static void MapPendingDoctorsEndpoint(
            this WebApplication app)
        {
            app.MapGet(
                "/api/admin/pending-doctors",
                async (
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetPendingDoctorsQuery(),
                        cancellationToken);

                    return Results.Ok(result);
                })
                .RequireAuthorization("AdminOnly");
        }
    }
}
