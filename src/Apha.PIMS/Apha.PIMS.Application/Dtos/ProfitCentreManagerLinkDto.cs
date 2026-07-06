/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.ProfitCentreManagerLink
 *   - Composite PK (Profitcentre, Manager) — both string — carried in DTO for Add/Delete operations
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.ProfitCentreManagerLink via EntityMapper; composite PK (Profitcentre, Manager)
    public class ProfitCentreManagerLinkDto
    {
        public string Profitcentre { get; set; } = null!;

        public string Manager { get; set; } = null!;
    }
}
