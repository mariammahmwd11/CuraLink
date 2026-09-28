using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Identity
{
    public interface IUserNameProvider
    {
        Task<string?> GetFullNameAsync(string applicationUserId, CancellationToken cancellationToken);
    }
}
