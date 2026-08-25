using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Features.Authentication.DTOs;
using CuraLink.Infrastructure.Authentication;
using CuraLink.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenGenerator jwtTokenGenerator;
        private readonly IRefreshTokenGenerator refreshTokenGenerator;

        public AuthService(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,IJwtTokenGenerator jwtTokenGenerator,IRefreshTokenGenerator refreshTokenGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.jwtTokenGenerator = jwtTokenGenerator;
            this.refreshTokenGenerator = refreshTokenGenerator;
        }
        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user =await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            var result =await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                throw new UnauthorizedAccessException(
                    "Account is locked.");
            }

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }
            var roles = await _userManager.GetRolesAsync(user);

            var role = roles.FirstOrDefault() ?? string.Empty;

            var accessToken = jwtTokenGenerator.GenerateToken(
                user.Id,
                user.Email!,
                role);

            var refreshToken = refreshTokenGenerator.GenerateToken();

            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Role = role
                }
            };
        }

    }
    
}
