using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsApiClient : IPimsApiClient
    {
        public IPimsProjectListApiClient PimsProjectList { get; }
        public IPimsProjectDetailsApiClient PimsProjectDetails { get; }
        public IPimsProjectCommentApiClient PimsProjectComment { get; }
        public IPimsProposedProjectApiClient PimsProposedProject { get; }
        public IPimsProjectYearCostsApiClient PimsProjectYearCosts { get; }

        public IPimsMilestoneApiClient PimsMilestone { get; }
        public IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }

        // TRANSFORMENGINE: New sub-client for YearlyFinancialData CRUD + pactcosts endpoint (Phase 9)
        public IPimsYearlyFinancialDataApiClient PimsYearlyFinancialData { get; }

        public PimsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            PimsProjectList = new PimsProjectListApiClient(http, mapper);
            PimsProjectDetails = new PimsProjectDetailsApiClient(http, mapper);
            PimsProjectComment = new PimsProjectCommentApiClient(http, mapper);
            PimsProposedProject = new PimsProposedProjectApiClient(http, mapper);
            PimsProjectYearCosts = new PimsProjectYearCostsApiClient(http, mapper);
            PimsMilestone = new PimsMilestoneApiClient(http, mapper);
            PimsRadTrackInvoice = new PimsRadTrackInvoiceApiClient(http, mapper);
            // TRANSFORMENGINE: Register YearlyFinancialDataApiClient on aggregate (Phase 9 — Step 14)
            PimsYearlyFinancialData = new PimsYearlyFinancialDataApiClient(http, mapper);
        }
    }
}
