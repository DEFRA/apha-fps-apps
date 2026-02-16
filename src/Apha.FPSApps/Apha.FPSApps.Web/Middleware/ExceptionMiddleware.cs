using Apha.FPSApps.Application.Validation;
using Microsoft.AspNetCore.Authentication;

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
                await _next(context); // Continue down the pipeline
            }
            catch (Exception ex)
            {
                string errorCode;
                string defaultErrorType = "<projectName>.GENERAL_EXCEPTION";
                string errorType = _configuration["ExceptionTypes:General"] ?? defaultErrorType;

                if (ex is UnauthorizedAccessException)
                {
                    errorCode = "403 - Forbidden";
                    errorType = _configuration["ExceptionTypes:Authorization"] ?? defaultErrorType;
                }
                else if (ex is AuthenticationFailureException)
                {
                    errorType = _configuration["ExceptionTypes:Authorization"] ?? defaultErrorType;
                    errorCode = "403 - Forbidden";
                }               
                else if (ex is BusinessValidationErrorException validationEx)
                {
                    context.Response.ContentType = "application/json";
                    errorCode = "400 -Bad Request Error";
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(validationEx.Errors);
                }
                else
                {
                    errorCode = "500 - Internal Server Error";
                }

                if (ex is UnauthorizedAccessException)
                {
                    var userid = context.User.Identity?.Name == null ? string.Empty : context.User.Identity?.Name;
                    _logger.LogError(ex, "[{ErrorType:l}] Error [{ErrorCode:l}]: {Message}", errorType, errorCode, userid + " ," + ex.Message);
                }
                else
                {
                    _logger.LogError(ex, "[{ErrorType:l}] Error [{ErrorCode:l}]: {Message}", errorType, errorCode, ex.Message);
                }

                context.Response.Redirect("/Error");
            }
        }
    }
}
