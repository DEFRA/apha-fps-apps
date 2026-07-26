using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsApiClient : IPimsApiClient
    {
        // TRANSFORMENGINE: existing Phase 5 sub-clients — preserved verbatim
        public IPimsProjectListApiClient PimsProjectList { get; }
        public IPimsProjectDetailsApiClient PimsProjectDetails { get; }
        public IPimsProjectCommentApiClient PimsProjectComment { get; }
        public IPimsProposedProjectApiClient PimsProposedProject { get; }
        public IPimsProjectYearCostsApiClient PimsProjectYearCosts { get; }
        public IPimsMilestoneApiClient PimsMilestone { get; }
        public IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }

        
        public IPimsYearlyFinancialDataApiClient PimsYearlyFinancialData { get; }


        // TRANSFORMENGINE: Phase 9 new sub-clients — maintenance form API surfaces
        public IPimsReportApiClient PimsReport { get; }
        public IPimsReportGroupApiClient PimsReportGroup { get; }
        public IPimsReportGroupLinkApiClient PimsReportGroupLink { get; }
        public IPimsProjectManagerApiClient PimsProjectManager { get; }
        public IPimsProgramManagerLinkApiClient PimsProgramManagerLink { get; }
        public IPimsProfitCentreManagerLinkApiClient PimsProfitCentreManagerLink { get; }
        public IPimsSettingApiClient PimsSetting { get; }
        public IPimsAccessUserApiClient PimsAccessUser { get; }
        public IPimsAccessLevelApiClient PimsAccessLevel { get; }
        public IPimsAccessUserLevelApiClient PimsAccessUserLevel { get; }
        public IPimsAccessSystemApiClient PimsAccessSystem { get; }
        public IPimsFrequencyApiClient PimsFrequency { get; }
        public IPimsReviewItemApiClient PimsReviewItem { get; }

        // TRANSFORMENGINE: PimsRadTrackProg — registered; binds to api/v1/radtrackprog (natural string PK, Programme Tab)
        public IPimsRadTrackProgApiClient PimsRadTrackProg { get; }

        // Risk Rating sub-client — binds to api/v1/risk-ratings
        public IPimsRiskApiClient PimsRisk { get; }

        // Publication Type sub-client — binds to api/v1/publication-types
        public IPimsPublicationTypeApiClient PimsPublicationType { get; }

        public PimsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            // TRANSFORMENGINE: existing Phase 5 sub-client initialisations — preserved verbatim
            PimsProjectList = new PimsProjectListApiClient(http, mapper);
            PimsProjectDetails = new PimsProjectDetailsApiClient(http, mapper);
            PimsProjectComment = new PimsProjectCommentApiClient(http, mapper);
            PimsProposedProject = new PimsProposedProjectApiClient(http, mapper);
            PimsProjectYearCosts = new PimsProjectYearCostsApiClient(http, mapper);
            PimsMilestone = new PimsMilestoneApiClient(http, mapper);
            PimsRadTrackInvoice = new PimsRadTrackInvoiceApiClient(http, mapper);
            
            PimsYearlyFinancialData = new PimsYearlyFinancialDataApiClient(http, mapper);

            // TRANSFORMENGINE: Phase 9 new sub-client initialisations
            PimsReport = new PimsReportApiClient(http, mapper);
            PimsReportGroup = new PimsReportGroupApiClient(http, mapper);
            PimsReportGroupLink = new PimsReportGroupLinkApiClient(http, mapper);
            PimsProjectManager = new PimsProjectManagerApiClient(http, mapper);
            PimsProgramManagerLink = new PimsProgramManagerLinkApiClient(http, mapper);
            PimsProfitCentreManagerLink = new PimsProfitCentreManagerLinkApiClient(http, mapper);
            PimsSetting = new PimsSettingApiClient(http, mapper);
            PimsAccessUser = new PimsAccessUserApiClient(http, mapper);
            PimsAccessLevel = new PimsAccessLevelApiClient(http, mapper);
            PimsAccessUserLevel = new PimsAccessUserLevelApiClient(http, mapper);
            PimsAccessSystem = new PimsAccessSystemApiClient(http, mapper);
            PimsFrequency = new PimsFrequencyApiClient(http, mapper);
            PimsReviewItem = new PimsReviewItemApiClient(http, mapper);

            // TRANSFORMENGINE: PimsRadTrackProg — wired to PimsRadTrackProgApiClient; api/v1/radtrackprog
            PimsRadTrackProg = new PimsRadTrackProgApiClient(http, mapper);

            // Risk Rating — wired to PimsRiskApiClient; api/v1/risk-ratings
            PimsRisk = new PimsRiskApiClient(http, mapper);

            // Publication Type — wired to PimsPublicationTypeApiClient; api/v1/publication-types
            PimsPublicationType = new PimsPublicationTypeApiClient(http, mapper);
        }
    }
}
