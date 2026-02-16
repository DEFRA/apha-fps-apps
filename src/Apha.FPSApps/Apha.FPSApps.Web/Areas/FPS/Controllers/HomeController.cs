using Apha.FPSApps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]    
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
    }
}
