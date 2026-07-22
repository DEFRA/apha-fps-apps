/*
 * TRANSFORMENGINE MIGRATION — IPimsApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Added TransformEngine migration annotation header
 *   - Verified IPimsProjectCommentApiClient PimsProjectComment property is present
 *
 * PRESERVED:
 *   - All existing sub-client properties: PimsProjectList, PimsProjectDetails,
 *     PimsProjectComment, PimsProposedProject, PimsProjectYearCosts,
 *     PimsMilestone, PimsRadTrackInvoice
 *   - Namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsApiClient
    {
        // TRANSFORMENGINE: aggregate PIMS API client — one sub-client property per backend controller
        IPimsProjectListApiClient PimsProjectList { get; }
        IPimsProjectDetailsApiClient PimsProjectDetails { get; }

        // TRANSFORMENGINE: IPimsProjectCommentApiClient — wires frontend to GET/POST/PUT/DELETE /api/v1/projectcomment
        IPimsProjectCommentApiClient PimsProjectComment { get; }

        IPimsProposedProjectApiClient PimsProposedProject { get; }
        IPimsProjectYearCostsApiClient PimsProjectYearCosts { get; }
        IPimsMilestoneApiClient PimsMilestone { get; }
        IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }
    }
}
