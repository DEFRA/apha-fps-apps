namespace Apha.FPSApps.Web.Handler
{
    public class RequestHeadersHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string FpsYearHeader = "X-FPS-Year";
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public RequestHeadersHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null && context.Items.TryGetValue("SelectedFPSYear", out var yearObj) && yearObj != null)
            {
                request.Headers.Remove(FpsYearHeader);
                request.Headers.Add(FpsYearHeader, yearObj.ToString());
            }

            var correlationId = context?.Request.Headers[CorrelationIdHeader].ToString();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = context?.TraceIdentifier ?? Guid.NewGuid().ToString();

            request.Headers.Remove(CorrelationIdHeader);
            request.Headers.Add(CorrelationIdHeader, correlationId);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
