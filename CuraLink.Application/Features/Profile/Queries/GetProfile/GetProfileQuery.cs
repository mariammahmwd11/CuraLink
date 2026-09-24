using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Profile.Queries.GetProfile
{
    public record GetProfileQuery(
     string UserId
 ) : IRequest<GetProfileResponse>;
}
