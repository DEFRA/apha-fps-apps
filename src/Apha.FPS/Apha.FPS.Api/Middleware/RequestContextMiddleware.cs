using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Api.Middleware
{
    public class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;
       
        private const string FpsYearHeader = "X-FPS-Year";
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public RequestContextMiddleware(
                RequestDelegate next)
        {
            _next = next;            
        }

        public async Task InvokeAsync(HttpContext context, IFpsRequestContext requestContext)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip swagger & health
            if (path != null &&
                (path.StartsWith("/swagger")
                 || path.StartsWith("/health")
                 || path.StartsWith("/favicon")))
            {
                await _next(context);
                return;
            }

            // REQUIRED HEADER
            if (!context.Request.Headers.TryGetValue(FpsYearHeader, out var header)
                            || !int.TryParse(header, out int fpsYear))
            {
                throw new ArgumentException($"Required request header '{FpsYearHeader}' is missing or empty.");
            }

            SetCorrelationId(context, CorrelationIdHeader);

            requestContext.FpsYear = fpsYear;
            requestContext.UserEmailId = ("Rohit.Agarwal@defradev.onmicrosoft.com").ToLowerInvariant(); //(context.User?.Identity?.Name ?? string.Empty).ToLowerInvariant();
            
            await _next(context);
        }

        private static void SetCorrelationId(HttpContext context, string CorrelationIdHeader)
        {
            // OPTIONAL HEADER (generate if missing)
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
                || string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeader] = correlationId;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId!;
                return Task.CompletedTask;
            });
        }

    }
}
