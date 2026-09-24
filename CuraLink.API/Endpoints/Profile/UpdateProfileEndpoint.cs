using System.Security.Claims;
using CuraLink.Application.Common.Models;
using CuraLink.Application.Features.Profile.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.API.Endpoints.Profile;

public static class UpdateProfileEndpoint
{
    public static void MapUpdateProfileEndpoint(
        this WebApplication app)
    {
        app.MapPut(
            "/api/profile",
            async (
                ClaimsPrincipal user,
                [FromForm] UpdateProfileRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                FileUpload? fileUpload = null;

                if (request.ProfilePhoto is not null)
                {
                    fileUpload = new FileUpload(
                        request.ProfilePhoto.OpenReadStream(),
                        request.ProfilePhoto.FileName,
                        request.ProfilePhoto.ContentType);
                }

                var command = new UpdateProfileCommand(
                    userId,
                    request.PhoneNumber,
                    request.Bio,
                    fileUpload);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization()
        .DisableAntiforgery();
    }
}