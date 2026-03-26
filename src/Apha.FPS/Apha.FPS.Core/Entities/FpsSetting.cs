namespace Apha.FPS.Core.Entities
{
    public partial class FpsSetting
    {
        public string Id { get; set; } = null!;

        public string? Setting { get; set; }

        public string? Notes { get; set; }

        public string? TestSetting { get; set; }

        public int? FpsYear { get; set; }
    }
}


