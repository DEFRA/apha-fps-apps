using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookApiClient : ICostBookApiClient
    {
        public ICostBookProjectApiClient Projects { get; }
        public ICostBookCustomerApiClient Customers { get; }
        public ICostBookDiseaseApiClient Diseases { get; }
        public ICostBookProgramApiClient Programs { get; }
        public ICostBookStaffApiClient Staff { get; }
        public ICostBookContractApiClient Contracts { get; }

        public CostBookApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            Projects = new CostBookProjectApiClient(http, mapper);
            Customers = new CostBookCustomerApiClient(http, mapper);
            Diseases = new CostBookDiseaseApiClient(http, mapper);
            Programs = new CostBookProgramApiClient(http, mapper);
            Staff = new CostBookStaffApiClient(http, mapper);
            Contracts = new CostBookContractApiClient(http, mapper);
        }
    }
}
