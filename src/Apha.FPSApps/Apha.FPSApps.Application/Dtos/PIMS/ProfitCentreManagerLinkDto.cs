/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.ProfitCentreManagerLinkDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Composite natural PK (Profitcentre, Manager) — both are required string fields
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.ProfitCentreManagerLinkDto — composite natural PK (Profitcentre, Manager)
    public class ProfitCentreManagerLinkDto
    {
        public string Profitcentre { get; set; } = null!;
        public string Manager { get; set; } = null!;
    }
}
