// Excerpt from ICM Solution (private repository) — ApiInfrastructure/Services/CurrentUserService.cs
// Tenant (church) and identity data always come from the validated JWT, never from the request body.

using System.Security.Claims;
using System.Text.Json;
using ApiInfrastructure.Interface;
using Microsoft.AspNetCore.Http;

namespace ApiInfrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
                return int.TryParse(userIdClaim, out _) ? userIdClaim : null;
            }
        }


        public string Username
        {
            get
            {
                var usernameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name);
                return usernameClaim?.Value ?? string.Empty;
            }
        }

        public string UserName
        {
            get
            {
                var userNameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userName") 
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name);
                return userNameClaim?.Value ?? string.Empty;
            }
        }

        public string UserEmail
        {
            get
            {
                var emailClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email);
                return emailClaim?.Value ?? string.Empty;
            }
        }

        public string UserRole
        {
            get
            {
                var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("roleId");
                return roleClaim?.Value ?? string.Empty;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        public IEnumerable<string> Roles
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null) return Enumerable.Empty<string>();

                var roleClaims = user.FindAll("roleId");
                return roleClaims.Select(claim => claim.Value);
            }
        }

        public IReadOnlyList<int> ChurchIds
        {
            get
            {
                var raw = _httpContextAccessor.HttpContext?.User?.FindFirst("churchIds")?.Value;
                if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<int>();

                try
                {
                    return JsonSerializer.Deserialize<List<int>>(raw) ?? new List<int>();
                }
                catch (JsonException)
                {
                    return raw.Trim('[', ']')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x.Trim(), out var id) ? id : 0)
                        .Where(id => id > 0)
                        .ToList();
                }
            }
        }
    }
}