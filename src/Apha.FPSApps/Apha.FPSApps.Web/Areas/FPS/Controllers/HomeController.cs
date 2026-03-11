using Apha.FPSApps.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    //[Authorize]
    [AllowAnonymous]//[Authorize(Roles = "FPSAdmin,FPSUser")]
    //[AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class HomeController : Controller
    {
        private readonly IWeatherForecastService _weatherForecastService;

        public HomeController(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _weatherForecastService.GetWeatherForecastAsync();
            if (response.Success)
            {
                ViewBag.Weather = response.Data;
            }

            return View();
        }
       
        public async Task<IActionResult> ExportToExcel()
        {
            var fileContent = await _weatherForecastService.ExportWeatherForecast();
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WeatherForecast.xlsx");
        }
    }
}
