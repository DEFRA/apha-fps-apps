using Apha.FPSApps.Web.Handler;

namespace Apha.FPSApps.Web.Middleware
{
    public class FpsYearMiddleware
    {
        private readonly RequestDelegate _next;


        public FpsYearMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
    HttpContext context,
    IFPSYearContext fyContext)
{
    int year;

    if (context.Request.Query.TryGetValue("year", out var q) && !string.IsNullOrEmpty(q))
    {
        year = int.Parse(q!);
    }
    else if (context.Request.Headers.TryGetValue("X-FPS-Year", out var h) && !string.IsNullOrEmpty(h))
    {
        year = int.Parse(h!);
    }
    else if (context.Request.HasFormContentType &&
             context.Request.Form.TryGetValue("FPSYear", out var f) && !string.IsNullOrEmpty(f))
    {
        year = int.Parse(f!);
    }
    else
    {                
        year = GetCurrentFPSYear();
    }

    fyContext.Year = year;
    context.Items["SelectedFPSYear"] = year;

    await _next(context);
}

        private int GetCurrentFPSYear()
        {
            var today = DateTime.Today;

            if (today.Month >= 4)   // April to Dec
                return 2025;//today.Year;
            else                    // Jan to March
                return today.Year - 1;
        }
    }
}
