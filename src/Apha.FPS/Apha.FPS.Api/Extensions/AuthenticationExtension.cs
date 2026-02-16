using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

namespace Apha.FPS.Api.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    configuration.Bind("AzureAd", options);
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = context => HandleTokenValidatedAsync(context),
                        OnRedirectToIdentityProvider = context => HandleRedirectToIdentityProvider(context),
                        OnRemoteFailure = context => HandleRemoteFailure(context)
                    };

                });

            return services;
        }

        private static Task HandleTokenValidatedAsync(TokenValidatedContext context)
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new UnauthorizedAccessException("Unauthorized Access");
            }

            var currentTime = DateTime.UtcNow;
            var userName = identity.Name! ?? string.Empty;
            var userroles = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Task.CompletedTask;
        }
        /// <summary>
        /// Ensures the redirect URI uses HTTPS instead of HTTP.
        /// This is typically used in OpenID Connect authentication
        /// to enforce secure redirection to the identity provider.
        /// </summary>
        /// <param name="context">
        /// The <see cref="RedirectContext"/> provided by the OIDC middleware.
        /// </param>
        /// <returns>A completed task.</returns>
        private static Task HandleRedirectToIdentityProvider(RedirectContext context)
        {
            // Check if the redirect URI starts with "http://"
            if (context.ProtocolMessage.RedirectUri?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Replace "http://" with "https://"
                context.ProtocolMessage.RedirectUri =
                    string.Concat("https://", context.ProtocolMessage.RedirectUri.AsSpan("http://".Length));
            }
            return Task.CompletedTask;
        }

        private static Task HandleRemoteFailure(RemoteFailureContext context)
        {
            if (context.Failure?.InnerException is UnauthorizedAccessException unauthorizedEx)
            {
                throw unauthorizedEx;
            }

            throw context.Failure ?? new Exception("Unknown authentication error.");
        }
    }
}
