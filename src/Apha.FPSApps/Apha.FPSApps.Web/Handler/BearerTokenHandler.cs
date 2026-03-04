using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace Apha.FPSApps.Web.Handler
{
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _scopes;


        public BearerTokenHandler(
             ITokenAcquisition tokenAcquisition,
             IHttpContextAccessor httpContextAccessor,
             string[] scopes)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpContextAccessor = httpContextAccessor;
            _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        }

        protected override async Task<HttpResponseMessage> SendAsync(
             HttpRequestMessage request,
             CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var accessToken = await _tokenAcquisition
                .GetAccessTokenForUserAsync(_scopes, user: user);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
