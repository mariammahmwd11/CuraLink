using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Clinics
{
    public static class GetDoctorClinicsEndpoint
    {
        public static void MapGetDoctorClinicsEndpoint(
       this WebApplication app)
        {
            app.MapGet(
                "/api/doctor/clinics",
                [Authorize(Roles = "Doctor")]
            async (
                    ClaimsPrincipal user,
                    ISender sender,
                    IDoctorRepository doctorRepository,
                    CancellationToken cancellationToken) =>
                {
                    var applicationUserId = user.FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(applicationUserId))
                        return Results.Unauthorized();

                    var doctor = await doctorRepository
                        .GetByApplicationUserIdAsync(
                            applicationUserId,
                            cancellationToken);

                    if (doctor is null)
                        return Results.NotFound("Doctor not found.");

                    var result = await sender.Send(
                        new GetAlldoctor_sClinicsQuery(doctor.Id),
                        cancellationToken);

                    return Results.Ok(result);
                }).WithTags("Clinics")
                .WithName("GetDoctorClinics");
        }
    }
}