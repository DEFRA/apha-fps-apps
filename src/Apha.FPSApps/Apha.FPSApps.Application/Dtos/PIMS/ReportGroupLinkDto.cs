/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.ReportGroupLinkDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Composite PK (Reportid, Groupid) — both fields are required
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.ReportGroupLinkDto — composite PK (Reportid, Groupid)
    public class ReportGroupLinkDto
    {
        public int Reportid { get; set; }
        public int Groupid { get; set; }
    }
}
