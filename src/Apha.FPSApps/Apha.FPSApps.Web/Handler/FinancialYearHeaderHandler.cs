namespace Apha.FPSApps.Web.Handler
{
    public class FinancialYearHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FinancialYearHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
               
            if (context != null && context.Items.TryGetValue("SelectedYear", out var yearObj) && yearObj != null)
            {
                request.Headers.Remove("X-Financial-Year");
                request.Headers.Add("X-Financial-Year", yearObj.ToString());
            }           

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
