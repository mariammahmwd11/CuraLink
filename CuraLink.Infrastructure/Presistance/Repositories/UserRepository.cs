using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features;
using CuraLink.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Infrastructure.Presistance.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserInfoDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;

        return new UserInfoDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!
        };
    }

    public async Task<List<UserInfoDto>> GetByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToList();

        return await _userManager.Users
            .Where(x => ids.Contains(x.Id))
            .Select(x => new UserInfoDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email!,
                PhoneNumber = x.PhoneNumber!
            })
            .ToListAsync(cancellationToken);
    }
}