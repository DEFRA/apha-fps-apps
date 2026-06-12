namespace Apha.Common.Contracts.FPS
{
    public class JobCodeRes
    {
        public string JobCodeId { get; set; } = null!;

        public string? JobCodeName { get; set; }

        public string? Type { get; set; }

        public string? ParentProject { get; set; }

        public int? FpsYear { get; set; }
    }
}
