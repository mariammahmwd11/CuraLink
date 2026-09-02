using CuraLink.Application.Features.Patients.Queries.SearchDoctors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.API.Endpoints.PatientEndpoints;

public static class SearchDoctorsEndpoint
{
    public static void MapSearchDoctorsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/patient/doctors",
            async (
                [FromQuery] string? doctorName,
                [FromQuery] string? specialty,
                [FromQuery] string? governorate,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                ISender sender,
                CancellationToken cancellationToken = default) =>
            {
                var query = new SearchDoctorsQuery(
                    doctorName,
                    specialty,
                    governorate,
                    pageNumber,
                    pageSize);

                var result = await sender.Send(
                    query,
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization("Patient");
    }
}