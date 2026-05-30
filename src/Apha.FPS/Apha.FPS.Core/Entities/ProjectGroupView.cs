namespace Apha.FPS.Core.Entities
{
    public partial class ProjectGroupView
    {
        public string ProjectGroupName { get; set; } = null!;

        public int? UserId { get; set; }

        public string? UserEmail { get; set; }
    }
}
