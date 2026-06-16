namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for creating or updating a Grade record.
    /// Contains only the writable ControlSource-bound fields from frmMaintGrade.
    /// </summary>
    public class GradeReq
    {
        // TRANSFORMENGINE: GradeCode — primary key field, required; maps to 'gradecode' (varchar 10) in fps.grade
        /// <summary>Grade code (primary key). Required.</summary>
        public string GradeCode { get; set; } = null!;

        // TRANSFORMENGINE: Description — maps to 'desc_long' (varchar 30); optional in HTML prototype
        /// <summary>Grade description. Maps to desc_long column.</summary>
        public string? Description { get; set; }

        // TRANSFORMENGINE: AvSalary — maps to 'avsalary' (money) in fps.grade; optional input
        /// <summary>Average salary. Maps to avsalary column.</summary>
        public decimal? AvSalary { get; set; }
    }
}
