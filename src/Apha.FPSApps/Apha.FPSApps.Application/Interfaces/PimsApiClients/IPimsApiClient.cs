namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsApiClient
    {
        IPimsProjectListApiClient PimsProjectList { get; }
        IPimsProjectDetailsApiClient PimsProjectDetails { get; }
        IPimsProjectCommentApiClient PimsProjectComment { get; }
        IPimsProposedProjectApiClient PimsProposedProject { get; }
        IPimsProjectYearCostsApiClient PimsProjectYearCosts { get; }
        IPimsMilestoneApiClient PimsMilestone { get; }
        IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }
        
        IPimsYearlyFinancialDataApiClient PimsYearlyFinancialData { get; }

        // TRANSFORMENGINE: Phase 7 sub-clients — maintenance form API surfaces (Report, ReportGroup, ReportGroupLink, ProjectManager, etc.)
        IPimsReportApiClient PimsReport { get; }
        IPimsReportGroupApiClient PimsReportGroup { get; }
        IPimsReportGroupLinkApiClient PimsReportGroupLink { get; }
        IPimsProjectManagerApiClient PimsProjectManager { get; }
        IPimsProgramManagerLinkApiClient PimsProgramManagerLink { get; }
        IPimsProfitCentreManagerLinkApiClient PimsProfitCentreManagerLink { get; }
        IPimsSettingApiClient PimsSetting { get; }
        IPimsAccessUserApiClient PimsAccessUser { get; }
        IPimsAccessLevelApiClient PimsAccessLevel { get; }
        IPimsAccessUserLevelApiClient PimsAccessUserLevel { get; }
        IPimsAccessSystemApiClient PimsAccessSystem { get; }
        IPimsFrequencyApiClient PimsFrequency { get; }

        // TRANSFORMENGINE: Phase 7 — ReviewItem and RadTrackProg sub-clients (Other Tab + Programme Tab CRUD)
        IPimsReviewItemApiClient PimsReviewItem { get; }
        IPimsRadTrackProgApiClient PimsRadTrackProg { get; }

        // Risk Rating sub-client (Other Tab CRUD)
        IPimsRiskApiClient PimsRisk { get; }

        // Publication Type sub-client (Other Tab CRUD)
        IPimsPublicationTypeApiClient PimsPublicationType { get; }
    }
}
