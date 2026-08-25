using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Authentication
{
    public interface IRefreshTokenGenerator
    {
        string GenerateToken();
    }
}
