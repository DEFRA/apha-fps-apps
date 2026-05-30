namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Represents a Grade record from fps.grade.
    /// </summary>
    public partial class Grade
    {
        /// <summary>Grade code (primary key component).</summary>
        public string GradeCode { get; set; } = null!;
        public string? DescLong { get; set; }

        /// <summary>FPS financial year (primary key component, filtered by HasQueryFilter).</summary>
        public int? FpsYear { get; set; }
    }
}
