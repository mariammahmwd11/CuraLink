using CuraLink.Application.Common.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Identity
{
    public class UserNameProvider : IUserNameProvider
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserNameProvider(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string?> GetFullNameAsync(string applicationUserId, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == applicationUserId, cancellationToken);


          
            return user?.FirstName + " " + user?.LastName;
        }
    }
}
