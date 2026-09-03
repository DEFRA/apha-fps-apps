namespace Apha.FPS.Core.Entities
{
    public partial class YearEndSettingStaging
    {
        public Guid JobQueueId { get; set; }
        public string Id { get; set; } = null!;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
    }
}
