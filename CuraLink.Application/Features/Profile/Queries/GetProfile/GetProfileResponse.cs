using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Profile.Queries.GetProfile
{
    public record GetProfileResponse(
    string UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Bio,
    string? ProfilePhotoUrl);
}
