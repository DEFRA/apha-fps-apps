using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                    return;

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Unwrap: UnauthorizedAccessException may wrap MicrosoftIdentityWebChallengeUserException
            var oidcChallenge = ex as MicrosoftIdentityWebChallengeUserException
                ?? ex.InnerException as MicrosoftIdentityWebChallengeUserException;

            //trigger an OIDC challenge to redirect to Azure AD.
            if (oidcChallenge != null)
            {
                // Redirect to Azure AD for re-authentication, returning to the current path
                await context.ChallengeAsync(
                    OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties
                    {
                        RedirectUri = context.Request.Path + context.Request.QueryString
                    });
                return;
            }

            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
            string errorCode = string.Empty;
            var errorType = _configuration["ExceptionTypes:General"]
                            ?? "FPS.GENERAL_EXCEPTION";

            switch (ex)
            {
                case UnauthorizedAccessException:
                case AuthenticationFailureException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    errorCode = "403 - Forbidden";
                    errorType = _configuration["ExceptionTypes:Authorization"];
                    break;
                case InvalidOperationException:
                case ArgumentException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    errorCode = "ARGUMENT_INVALID";
                    break;
                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    errorCode = "RESOURCE_NOT_FOUND";
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    errorCode = "SERVER_500";
                    break;
            }

            LogException(context, ex, errorType!, errorCode, correlationId);

            //Pending to create a user friendly error page and redirect to that page
            if (!context.Request.Path.StartsWithSegments("/Home/Error"))
            {
                context.Response.Redirect("/Home/Error");
            }

            await context.Response.CompleteAsync();
        }

        private void LogException(
          HttpContext context,
          Exception ex,
          string errorType,
          string errorCode,
          string correlationId)
        {
            var userId = context.User.Identity?.Name ?? "Anonymous";

            _logger.LogError(
                ex,
                "[{ErrorType}] [{ErrorCode}] User:{UserId} CorrelationId:{CorrelationId} Message:{Message}",
                errorType,
                errorCode,
                userId,
                correlationId,
                ex.Message);
        }
    }
}
