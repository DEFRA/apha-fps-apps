using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsApiClient : IFpsApiClient
    {
        public IFpsStaffJobApiClient FpsStaffJob { get; }

        public IFpsEmployeeApiClient FpsEmployee { get; }

        public IFpsProgramApiClient FpsProgram { get; }
        public FpsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {            
            FpsStaffJob = new FpsStaffJobApiClient(http, mapper);
            FpsEmployee = new FpsEmployeeApiClient(http, mapper);
            FpsProgram = new FpsProgramApiClient(http, mapper);
        }
    }
}
