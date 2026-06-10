namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing the fps.vpvtprojectgroupmgrplan view.
    /// Provides a pivot summary of planned staff costs grouped by project group and manager.
    /// </summary>
    public class ProjectGroupStaffPlanView
    {
        public string? ProjectGroup { get; set; }
        public string? ResourceCentre { get; set; }
        public string? WorkGroup { get; set; }
        public string? GradeCode { get; set; }
        public string? Name { get; set; }
        public string? Manager { get; set; }
        public string? JobCode { get; set; }
        public string? ProjectStatus { get; set; }
        public double? Hrs { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? Fee { get; set; }
        public int? FpsYear { get; set; }
    }
}
