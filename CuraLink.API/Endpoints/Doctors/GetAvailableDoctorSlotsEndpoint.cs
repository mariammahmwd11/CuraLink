
using CuraLink.Application.Features.Doctors.Queries.GetAvailableDoctorSlots;
using MediatR;

namespace CuraLink.API.Endpoints.Doctors;

public static class GetAvailableDoctorSlotsEndpoint
{
    public static void MapGetAvailableDoctorSlotsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/doctors/{doctorId:guid}/available-slots",
            async (
                Guid doctorId,
                DateTime date,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetAvailableDoctorSlotsQuery(
                        doctorId,
                        date),
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization().WithTags("Doctors");
    }
}

