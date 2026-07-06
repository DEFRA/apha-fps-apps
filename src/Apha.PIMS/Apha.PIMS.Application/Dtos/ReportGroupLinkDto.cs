/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.ReportGroupLink
 *   - Composite PK (Reportid, Groupid) carried in DTO to support Add/Delete operations
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.ReportGroupLink via EntityMapper; composite PK (Reportid, Groupid)
    public class ReportGroupLinkDto
    {
        public int Reportid { get; set; }

        public int Groupid { get; set; }
    }
}
