using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Authentication
{

    public interface ICurrentUserService
    {
        string? UserId { get; }

        string? Role { get; }

        bool IsAuthenticated { get; }
    }
}
