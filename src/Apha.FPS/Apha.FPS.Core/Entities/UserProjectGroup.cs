namespace Apha.FPS.Core.Entities
{
    public partial class UserProjectGroup
    {
        public int UserId { get; set; }

        public string ProjectGroup { get; set; } = null!;

        public int? FpsYear { get; set; }
    }
}
