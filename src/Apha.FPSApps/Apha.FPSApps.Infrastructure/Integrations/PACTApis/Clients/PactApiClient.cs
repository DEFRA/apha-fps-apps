using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactApiClient : IPactApiClient
    {
        public IPactJobCodeApiClient PactJobCode { get; }
        public IPactTimeCodeValidApiClient PactTimeCodeValid { get; }
        public IPactWorkGroupApiClient PactWorkGroup { get; }
        public IPactProjectInvoiceApiClient PactProjectInvoice { get; }
        public IPactProjectSubContractApiClient PactProjectSubContract { get; }
        public IPactWorkGroupTestCapabilityApiClient PactWorkGroupTestCapability { get; }
        public IPactTestRequirementApiClient PactTestRequirement { get; }

        public PactApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            PactJobCode = new PactJobCodeApiClient(http, mapper);
            PactTimeCodeValid = new PactTimeCodeValidApiClient(http, mapper);
            PactWorkGroup = new PactWorkGroupApiClient(http, mapper);
            PactProjectInvoice = new PactProjectInvoiceApiClient(http, mapper);
            PactProjectSubContract = new PactProjectSubContractApiClient(http, mapper);
            PactWorkGroupTestCapability = new PactWorkGroupTestCapabilityApiClient(http, mapper);
            PactTestRequirement = new PactTestRequirementApiClient(http, mapper);
        }
    }
}
