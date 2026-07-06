/*
 * TRANSFORMENGINE MIGRATION — ReportGroupDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.ReportGroupDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Used as lookup dropdown DTO for the Reports Tab report-group selector
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.ReportGroupDto — lookup shape for dropdown population
    public class ReportGroupDto
    {
        public int Groupid { get; set; }
        public string Description { get; set; } = null!;
    }
}
