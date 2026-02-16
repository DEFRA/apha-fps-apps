namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsApiClient
    {
        IFpsWeatherForecastApiClient FpsWeatherForecast { get; }
    }
}
