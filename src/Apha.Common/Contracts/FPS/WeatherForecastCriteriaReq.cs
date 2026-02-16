namespace Apha.Common.Contracts.FPS
{
    public class WeatherForecastCriteriaReq
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string? Summary { get; set; }
    }
}
