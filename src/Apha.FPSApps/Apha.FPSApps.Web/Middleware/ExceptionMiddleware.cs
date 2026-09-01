using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using System.Net.Sockets;

namespace Apha.FPSApps.Web.Middleware
{
    public class ExceptionMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
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
            var correlationId = EnsureCorrelationId(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (errorType, errorCode, statusCode) = ClassifyException(ex);
                LogException(context, ex, errorType, errorCode, correlationId);

                if (context.Response.HasStarted)
                    return;

                await HandleExceptionAsync(context, ex, statusCode);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, int statusCode)
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

            context.Response.StatusCode = statusCode;

            //Pending to create a user friendly error page and redirect to that page
            if (!context.Request.Path.StartsWithSegments("/Error"))
            {
                context.Response.Redirect("/Error/Index");
            }

            await context.Response.CompleteAsync();
        }

        private (string errorType, string errorCode, int statusCode) ClassifyException(Exception ex)
        {
            var errorType = _configuration["ExceptionTypes:General"]
                            ?? "FPSAPPS.EXCEPTION.WEB.GENERAL";
            string errorCode;
            int statusCode;

            switch (ex)
            {
                case UnauthorizedAccessException:
                case AuthenticationFailureException:
                    errorCode = "403 - Forbidden";
                    statusCode = StatusCodes.Status403Forbidden;
                    errorType = _configuration["ExceptionTypes:Authorization"] ?? errorType;
                    break;
                case HttpRequestException httpEx when httpEx.InnerException is SocketException socketEx && socketEx.ErrorCode == 10061:
                    errorCode = "SERVICE_UNAVAILABLE";
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    break;
                case InvalidOperationException:
                case ArgumentException:
                    errorCode = "ARGUMENT_INVALID";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case KeyNotFoundException:
                    errorCode = "RESOURCE_NOT_FOUND";
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                default:
                    errorCode = "SERVER_500";
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            return (errorType, errorCode, statusCode);
        }

        private static string EnsureCorrelationId(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationIdHeader].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            context.Request.Headers[CorrelationIdHeader] = correlationId;
            context.TraceIdentifier = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            return correlationId;
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
