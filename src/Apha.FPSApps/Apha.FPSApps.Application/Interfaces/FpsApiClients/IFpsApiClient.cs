namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsApiClient
    {
        IFpsWeatherForecastApiClient FpsWeatherForecast { get; }
        IFpsStaffJobApiClient FpsStaffJob { get; }
        IFpsEmployeeApiClient FpsEmployee { get; }
        IFpsProgramApiClient FpsProgram { get; }
    }
}
