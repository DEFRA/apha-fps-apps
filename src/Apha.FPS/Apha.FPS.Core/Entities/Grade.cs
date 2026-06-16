namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Represents a Grade record from fps.grade.
    /// Composite primary key: (GradeCode, FpsYear).
    /// FpsYear is additionally filtered via HasQueryFilter in FpsDbContext.
    /// </summary>
    public partial class Grade
    {
        /// <summary>Grade code (primary key component). Maps to fps.grade.gradecode.</summary>
        public string GradeCode { get; set; } = null!;

        /// <summary>Long description. Maps to fps.grade.desc_long.</summary>
        public string? DescLong { get; set; }

        // TRANSFORMENGINE: Added — maps to fps.grade.avsalary (money DEFAULT 0)
        /// <summary>Average salary. Maps to fps.grade.avsalary.</summary>
        public decimal? AvSalary { get; set; }

        // TRANSFORMENGINE: Added — maps to fps.grade.pactcode (varchar(50)); present in DDL but not in HTML prototype
        /// <summary>PACT system code. Maps to fps.grade.pactcode.</summary>
        public string? PactCode { get; set; }

        // TRANSFORMENGINE: Added — maps to fps.grade.avleavehrs (double precision DEFAULT 0); DDL-only field
        /// <summary>Average leave hours. Maps to fps.grade.avleavehrs.</summary>
        public double? AvLeaveHrs { get; set; }

        // TRANSFORMENGINE: Added — maps to fps.grade.avsickhrs (double precision DEFAULT 0); DDL-only field
        /// <summary>Average sick hours. Maps to fps.grade.avsickhrs.</summary>
        public double? AvSickHrs { get; set; }

        /// <summary>FPS financial year (primary key component, filtered by HasQueryFilter). Maps to fps.grade.fpsyear.</summary>
        public int? FpsYear { get; set; }
    }
}
