// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — GradeDto.cs (new file)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New DTO created — service layer contract for Grade entity (fps.grade table)
 *   - Description property maps to Grade.DescLong (renamed for UI clarity; handled by EntityMapper ForMember)
 *   - Includes all entity fields: GradeCode, Description, AvSalary, PactCode, AvLeaveHrs, AvSickHrs, FpsYear
 *
 * PRESERVED:
 *   - All field names visible in the HTML prototype: GradeCode, Description (DescLong), AvSalary
 *   - FpsYear included for year-aware service operations
 *   - DDL-only fields (PactCode, AvLeaveHrs, AvSickHrs) retained for full entity coverage
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether PactCode, AvLeaveHrs, AvSickHrs need UI exposure
 *     in the GradeMaintenance form (_AddEditGrade.cshtml).
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data transfer object for the Grade entity (fps.grade).
    /// Used as the service layer contract between GradeService and API/repository layers.
    /// Composite key: GradeCode + FpsYear (FpsYear enforced via HasQueryFilter in DbContext).
    /// </summary>
    public class GradeDto
    {
        /// <summary>Grade code (primary key component). Maps to fps.grade.gradecode.</summary>
        public string GradeCode { get; set; } = null!;

        // TRANSFORMENGINE: Description maps to Grade.DescLong; renamed for UI clarity — EntityMapper uses ForMember
        /// <summary>Long description. Maps to fps.grade.desc_long (Grade.DescLong).</summary>
        public string? Description { get; set; }

        /// <summary>Average salary. Maps to fps.grade.avsalary.</summary>
        public decimal? AvSalary { get; set; }

        // TRANSFORMENGINE: DDL-only field — not in HTML prototype; retained for full entity coverage
        /// <summary>PACT system code. Maps to fps.grade.pactcode.</summary>
        public string? PactCode { get; set; }

        // TRANSFORMENGINE: DDL-only field — not in HTML prototype; retained for full entity coverage
        /// <summary>Average leave hours. Maps to fps.grade.avleavehrs.</summary>
        public double? AvLeaveHrs { get; set; }

        // TRANSFORMENGINE: DDL-only field — not in HTML prototype; retained for full entity coverage
        /// <summary>Average sick hours. Maps to fps.grade.avsickhrs.</summary>
        public double? AvSickHrs { get; set; }

        /// <summary>FPS financial year (primary key component). Maps to fps.grade.fpsyear.</summary>
        public int? FpsYear { get; set; }
    }
}
