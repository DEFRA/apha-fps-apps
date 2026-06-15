// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IPimsApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - Added IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice property to register
 *     the new RadTrack Invoice API client on the aggregate PIMS client interface.
 *
 * PRESERVED:
 *   - All existing sub-client properties: PimsProjectList, PimsProjectDetails,
 *     PimsProjectComment, PimsProposedProject, PimsProjectYearCosts, PimsMilestone.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PimsApiClient.cs (Phase 9) must initialize the
 *     PimsRadTrackInvoice property with a PimsRadTrackInvoiceApiClient instance.
 */

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

        // TRANSFORMENGINE: New property — registers IPimsRadTrackInvoiceApiClient on the
        // aggregate PIMS client. Implemented in PimsApiClient.cs (Phase 9).
        IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }
    }
}
