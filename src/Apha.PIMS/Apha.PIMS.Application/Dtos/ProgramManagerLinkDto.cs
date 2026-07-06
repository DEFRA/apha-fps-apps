/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLinkDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.ProgramManagerLink
 *   - Composite PK (Program, Manager) — both string — carried in DTO for Add/Delete operations
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.ProgramManagerLink via EntityMapper; composite PK (Program, Manager)
    public class ProgramManagerLinkDto
    {
        public string Program { get; set; } = null!;

        public string Manager { get; set; } = null!;
    }
}
