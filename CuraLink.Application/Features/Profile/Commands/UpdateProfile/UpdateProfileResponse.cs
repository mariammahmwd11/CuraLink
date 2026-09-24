using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Profile.Commands.UpdateProfile
{
    public record UpdateProfileResponse(
     string UserId,
     string FirstName,
     string LastName,
     string? PhoneNumber,
     string? Bio,
     string? ProfilePhotoUrl);
}
