using CuraLink.Application.Common.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CuraLink.Infrastructure.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue("UserId");

        public string? Role =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.Role);

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?
                .User?
                .Identity?
                .IsAuthenticated ?? false;
    }
}