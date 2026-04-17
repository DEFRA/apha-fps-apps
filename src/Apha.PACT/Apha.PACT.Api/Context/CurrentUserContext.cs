using Apha.PACT.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Apha.PACT.Api.Context
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
            _httpContextAccessor.HttpContext?.User?.Identity?.Name
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value
            ?? "SYSTEM";
    }
}
