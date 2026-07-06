/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application DTO mirroring Apha.PIMS.Core.Entities.ProjectManager
 *   - String PK (Projectmanager name) — not identity-generated
 *
 * PRESERVED:
 *   - All field names consistent with entity naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.PIMS.Application.Dtos
{
    // TRANSFORMENGINE: DTO maps to/from Apha.PIMS.Core.Entities.ProjectManager via EntityMapper; string PK (Projectmanager)
    public class ProjectManagerDto
    {
        public string Projectmanager { get; set; } = null!;

        public string? Email { get; set; }

        public string? Mnumber { get; set; }

        public bool Disable { get; set; }
    }
}
