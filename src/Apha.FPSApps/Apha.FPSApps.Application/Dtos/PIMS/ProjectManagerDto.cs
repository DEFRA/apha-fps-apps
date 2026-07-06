/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend DTO mirroring Apha.PIMS.Application.Dtos.ProjectManagerDto
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for frontend consumption
 *   - Natural varchar PK (Projectmanager) preserved
 *
 * PRESERVED:
 *   - All property names match backend DTO exactly (case-sensitive)
 *   - Nullable reference types match backend DTO nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Frontend DTO mirroring Apha.PIMS.Application.Dtos.ProjectManagerDto — natural varchar PK (Projectmanager)
    public class ProjectManagerDto
    {
        public string Projectmanager { get; set; } = null!;
        public string? Email { get; set; }
        public string? Mnumber { get; set; }
        public bool Disable { get; set; }
    }
}
