namespace Apha.Common.Contracts.FPS
{
    public class FpsYearEndSettingRes
    {
        public string Id { get; set; } = string.Empty;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public int? FpsCalYear { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FpsYearType { get; set; } = string.Empty;
    }
}
