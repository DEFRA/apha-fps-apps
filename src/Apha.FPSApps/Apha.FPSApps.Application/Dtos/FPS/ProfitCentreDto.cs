namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ProfitCentreDto
    {
        public string ProfitCentreId { get; set; } = null!;
        public string ProfitCentreName { get; set; } = null!;
        public string? Division { get; set; }
        public decimal? ContTarget { get; set; }
        public string? ProfitCentreHead { get; set; }
        public int? DivisionId { get; set; }
        public string? EmailRecipient { get; set; }
    }
}
