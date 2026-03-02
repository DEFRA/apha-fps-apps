using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Context;

namespace Apha.PIMS.Api.Middleware
{
    public class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;        
        private const string FpsYearHeader = "X-FPS-Year";
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public RequestContextMiddleware(
                RequestDelegate next,
                ILogger<RequestContextMiddleware> logger)
        {
            _next = next;            
        }

        public async Task InvokeAsync(HttpContext context, IFPSYearContext yearContext)
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

            ((FPSYearContext)yearContext).FPSYear = fpsYear;

            await _next(context);
        }

        private void SetCorrelationId(HttpContext context, string CorrelationIdHeader)
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
