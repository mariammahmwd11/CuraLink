using CuraLink.Application.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IUserRepository
    {
        Task<UserInfoDto?> GetByIdAsync(
            string userId,
            CancellationToken cancellationToken = default);
        Task<List<UserInfoDto>> GetByIdsAsync(
         IEnumerable<string> userIds,
         CancellationToken cancellationToken = default);
    }
}
