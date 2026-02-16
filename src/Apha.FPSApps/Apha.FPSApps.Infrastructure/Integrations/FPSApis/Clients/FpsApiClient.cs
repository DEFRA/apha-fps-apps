using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsApiClient : IFpsApiClient
    {
        public IFpsWeatherForecastApiClient FpsWeatherForecast { get; }

        public FpsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            FpsWeatherForecast = new FpsWeatherForecastApiClient(http, mapper);
        }
    }
}
