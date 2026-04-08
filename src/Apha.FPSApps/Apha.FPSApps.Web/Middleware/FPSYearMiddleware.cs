using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Enums;
using System.Linq;
using Apha.FPSApps.Application.Interfaces.FPS;

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
    IFpsYearContext fyContext,
    IYearMasterService yearMasterService)
{
    int year;
    YearStatus yearStatus = YearStatus.Closed;

    // Set a temporary year in context to allow API calls to work
    var tempYear = GetCurrentFPSYear();
    context.Items["SelectedFPSYear"] = tempYear;



    // Get all year masters once - single DB call
    try
    {
        var allYearsResponse = await yearMasterService.GetAllFpsYearsAsync();
        var allYears = allYearsResponse?.Data;

        if (allYears != null)
        {
            // Determine which year to use
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
                // Find the year with "Open" status 
                var openYear = allYears.FirstOrDefault(y => y.YearStatus?.Equals("Open", StringComparison.OrdinalIgnoreCase) == true);
                year = openYear?.FpsYear ?? tempYear;
            }

            // Get the year status from the already loaded data
            var selectedYear = allYears.FirstOrDefault(y => y.FpsYear == year);
            yearStatus = selectedYear?.YearStatus == null ? YearStatus.Closed : (YearStatus) Enum.Parse(typeof(YearStatus), selectedYear.YearStatus, true);
        }
        else
        {
            year = tempYear;
        }
    }
    catch
    {
        year = tempYear;
    }

    fyContext.Year = year;
    context.Items["SelectedFPSYear"] = year;

    // Get area from route data (available after UseRouting())
    var area = context.GetRouteData()?.Values["area"]?.ToString();

    // Set IsReadOnly based on YearStatus and Area
    if (area?.Equals("FPS", StringComparison.OrdinalIgnoreCase) == true)
    {
        // For FPS area
        if (yearStatus != YearStatus.Closed)
        {
            // Editable if status is "planned" or "open"
            fyContext.IsReadOnly = !(yearStatus == YearStatus.Planned || yearStatus == YearStatus.Open);
        }
        else
        {
            // Default to readonly for FPS if status unknown
            fyContext.IsReadOnly = true;
        }
    }
    else if (area?.Equals("PACT", StringComparison.OrdinalIgnoreCase) == true)
    {
        // For PACT area
        if (yearStatus != YearStatus.Closed)
        {
            // Editable only if status is "open"
            fyContext.IsReadOnly = yearStatus != YearStatus.Open;
        }
        else
        {
            // Default to readonly for PACT if status unknown
            fyContext.IsReadOnly = true;
        }
    }
    else
    {
        // For all other areas: always editable
        fyContext.IsReadOnly = false;
    }

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
