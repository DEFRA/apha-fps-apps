/*
 * TRANSFORMENGINE MIGRATION — PimsApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - TransformEngine migration annotation header added
 *
 * PRESERVED:
 *   - All 7 aggregate client properties: PimsProjectList, PimsProjectDetails, PimsProjectComment,
 *     PimsProposedProject, PimsProjectYearCosts, PimsMilestone, PimsRadTrackInvoice
 *   - All constructor assignments wiring each I[App]XxxApiClient implementation
 *   - IPimsApiClient interface implementation
 *   - PimsProjectComment property wired to PimsProjectCommentApiClient (confirmed present)
 *   - Namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
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
        public IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }
        public PimsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            PimsProjectList = new PimsProjectListApiClient(http, mapper);
            PimsProjectDetails = new PimsProjectDetailsApiClient(http, mapper);
            PimsProjectComment = new PimsProjectCommentApiClient(http, mapper);
            PimsProposedProject = new PimsProposedProjectApiClient(http, mapper);
            PimsProjectYearCosts = new PimsProjectYearCostsApiClient(http, mapper);
            PimsMilestone = new PimsMilestoneApiClient(http, mapper);
            PimsRadTrackInvoice = new PimsRadTrackInvoiceApiClient(http, mapper);
        }
    }
}
