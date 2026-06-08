
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
