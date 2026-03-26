namespace Apha.FPS.Core.Entities
{
    public partial class JobCode
    {
        public string JobCodeId { get; set; } = null!;

        public string? ParentProject { get; set; }

        public string? JobCodeWorkGroup { get; set; }

        public string? NewProg { get; set; }

        public string? Type { get; set; }

        public string? JobCodeName { get; set; }

        public int? FpsYear { get; set; }
    }
}
