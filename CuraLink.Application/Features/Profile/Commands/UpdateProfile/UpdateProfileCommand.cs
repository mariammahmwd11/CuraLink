using CuraLink.Application.Common.Models;
using MediatR;


namespace CuraLink.Application.Features.Profile.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string UserId,
    string? PhoneNumber,
    string? Bio,
    Common.Models.FileUpload? ProfilePhoto
) : IRequest<UpdateProfileResponse>;