// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — GradeDto.cs (frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New frontend DTO created in Apha.FPSApps.Application.Dtos.FPS namespace
 *   - Mirrors Apha.FPS.Application.Dtos.GradeDto (backend Application layer DTO)
 *   - Same property names and types; different namespace for frontend isolation
 *   - All seven backend DTO fields included: GradeCode, Description, AvSalary, PactCode,
 *     AvLeaveHrs, AvSickHrs, FpsYear
 *
 * PRESERVED:
 *   - Property names exactly match backend GradeDto (case-sensitive, required by ApiDtoMapper)
 *   - Nullability mirrors backend: GradeCode non-nullable (PK), FpsYear nullable int?, all others nullable
 *   - Description property retains DescLong → Description rename established in backend EntityMapper
 *   - DDL-only fields (PactCode, AvLeaveHrs, AvSickHrs) retained for full entity surface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether PactCode, AvLeaveHrs, AvSickHrs need exposure in
 *     the _AddEditGrade.cshtml Razor partial view (currently carried but not bound to any form input).
 *   - TRANSFORMENGINE TODO: FpsApiDtoMapper must register GradeDto <-> GradeRes and GradeDto <-> GradeReq
 *     mappings; verify these entries exist in Apha.FPSApps.Infrastructure before running.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for the Grade entity (fps.grade).
    /// Same shape as Apha.FPS.Application.Dtos.GradeDto.
    /// Used as the service/API-client contract in the FPSApps frontend application layer.
    /// Composite key: GradeCode + FpsYear (FpsYear partition enforced server-side via HasQueryFilter).
    /// </summary>
    public class GradeDto
    {
        // TRANSFORMENGINE: PK component — maps to fps.grade.gradecode; required (non-nullable)
        /// <summary>Grade code (primary key component). Maps to fps.grade.gradecode.</summary>
        public string GradeCode { get; set; } = null!;

        // TRANSFORMENGINE: Description maps to Grade.DescLong in backend entity — rename handled by backend EntityMapper
        /// <summary>Long description. Maps to fps.grade.desc_long (via backend Grade.DescLong rename).</summary>
        public string? Description { get; set; }

        /// <summary>Average salary. Maps to fps.grade.avsalary.</summary>
        public decimal? AvSalary { get; set; }

        // TRANSFORMENGINE: DDL-only field — not exposed in HTML prototype; retained for full entity coverage
        /// <summary>PACT system code. Maps to fps.grade.pactcode.</summary>
        public string? PactCode { get; set; }

        // TRANSFORMENGINE: DDL-only field — not exposed in HTML prototype; retained for full entity coverage
        /// <summary>Average leave hours. Maps to fps.grade.avleavehrs.</summary>
        public double? AvLeaveHrs { get; set; }

        // TRANSFORMENGINE: DDL-only field — not exposed in HTML prototype; retained for full entity coverage
        /// <summary>Average sick hours. Maps to fps.grade.avsickhrs.</summary>
        public double? AvSickHrs { get; set; }

        // TRANSFORMENGINE: PK component — FpsYear partition key; nullable to allow service-level year injection
        /// <summary>FPS financial year (primary key component). Maps to fps.grade.fpsyear.</summary>
        public int? FpsYear { get; set; }
    }
}
