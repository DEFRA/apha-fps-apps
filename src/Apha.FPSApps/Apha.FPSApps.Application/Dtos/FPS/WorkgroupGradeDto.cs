/*
 * TRANSFORMENGINE MIGRATION — WorkgroupGradeDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend DTO created mirroring Apha.FPS.Application.Dtos.WorkgroupGradeDto
 *   - Namespace scoped to Apha.FPSApps.Application.Dtos.FPS (frontend application layer)
 *   - Used as both CRUD DTO (workgroup grade maintenance) and filter source (WgGrade → staff grid filter
 *     in Set Up Staff Resources — GetWorkGroupGradeAsync returns list filtered by profitCentre param)
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (WgGrade, ProfitCentreGrade, GradeCode, Workgroup, FpsYear)
 *   - Property types and nullability annotations preserved verbatim
 *   - XML doc-comment summaries preserved for developer context
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Re-verify field parity if backend WorkgroupGradeDto gains new columns
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for a WorkgroupGrade record.
    /// </summary>
    public class WorkgroupGradeDto
    {
        /// <summary>WG Grade code (primary key).</summary>
        public string WgGrade { get; set; } = null!;

        /// <summary>Profit Centre Grade code.</summary>
        public string ProfitCentreGrade { get; set; } = null!;

        /// <summary>Grade code.</summary>
        public string GradeCode { get; set; } = null!;

        /// <summary>Workgroup name.</summary>
        public string Workgroup { get; set; } = null!;

        /// <summary>FPS financial year.</summary>
        public int? FpsYear { get; set; }
    }
}
