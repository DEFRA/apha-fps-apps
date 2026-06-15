// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — PimsApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; } property to expose
 *     the new PimsRadTrackInvoiceApiClient through the aggregate client facade.
 *   - Initialized PimsRadTrackInvoice in constructor via new PimsRadTrackInvoiceApiClient(http, mapper).
 *   - This resolves the PENDING Interface Changes Log entry noted in transform-plan.md.
 *
 * PRESERVED:
 *   - All existing sub-client properties: PimsProjectList, PimsProjectDetails,
 *     PimsProjectComment, PimsProposedProject, PimsProjectYearCosts, PimsMilestone.
 *   - Constructor parameter signature: (IPimsHttpExecutor http, IMapper mapper).
 *   - All existing constructor assignments unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that the DI registration for IPimsApiClient / PimsApiClient
 *     in Apha.FPSApps.Infrastructure ServiceCollectionExtension still resolves correctly
 *     after this property addition (no new registration needed; PimsApiClient is the
 *     concrete type that composes all sub-clients in one constructor).
 */

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
        // TRANSFORMENGINE: New property — exposes PimsRadTrackInvoiceApiClient (Phase 9).
        // Satisfies the IPimsApiClient.PimsRadTrackInvoice property added in Phase 7.
        public IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }

        public PimsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            PimsProjectList = new PimsProjectListApiClient(http, mapper);
            PimsProjectDetails = new PimsProjectDetailsApiClient(http, mapper);
            PimsProjectComment = new PimsProjectCommentApiClient(http, mapper);
            PimsProposedProject = new PimsProposedProjectApiClient(http, mapper);
            PimsProjectYearCosts = new PimsProjectYearCostsApiClient(http, mapper);
            PimsMilestone = new PimsMilestoneApiClient(http, mapper);
            // TRANSFORMENGINE: Initialize PimsRadTrackInvoice — resolves Interface Changes Log PENDING entry.
            PimsRadTrackInvoice = new PimsRadTrackInvoiceApiClient(http, mapper);
        }
    }
}
