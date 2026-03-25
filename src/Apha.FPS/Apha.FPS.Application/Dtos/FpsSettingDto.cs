namespace Apha.FPS.Application.Dtos
{
    public class FpsSettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public string? TestSetting { get; set; }
        public int? FpsCalYear { get; set; }
    }
}
