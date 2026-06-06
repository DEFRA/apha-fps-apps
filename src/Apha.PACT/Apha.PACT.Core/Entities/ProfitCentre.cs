namespace Apha.PACT.Core.Entities
{
    public partial class ProfitCentre
    {
        public string ProfitCentreId { get; set; } = null!;

        public string ProfitCentreName { get; set; } = null!;

        public string Division { get; set; } = null!;

        public decimal? ContTarget { get; set; }

        public string? ProfitCentreHead { get; set; }

        public int? DivisionId { get; set; }

        public string? EmailRecipient { get; set; }

        public short? Timesheetlayout { get; set; }

        public int? Timesheet { get; set; }

        public int? Outputsheet { get; set; }

        public string? PactCoordinatorEmailName { get; set; }

        public byte[]? HighLevelSummary { get; set; }
    }
}
