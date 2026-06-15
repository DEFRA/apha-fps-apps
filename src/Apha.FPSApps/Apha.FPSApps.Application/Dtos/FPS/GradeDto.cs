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
