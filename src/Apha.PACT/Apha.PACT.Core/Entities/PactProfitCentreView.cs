namespace Apha.PACT.Core.Entities
{
    public partial class PactProfitCentreView
    {
        public string? ProfitCentre { get; set; }

        public string? ProfitCentreName { get; set; }

        public string? Division { get; set; }

        public decimal? ContTarget { get; set; }

        public string? ProfitCentreHead { get; set; }

        public int? DivisionId { get; set; }
        public string? EmailRecipient { get; set; }

        public string? PactCoordinatorEmailName { get; set; }

        public int? Timesheet { get; set; }

        public int? Outputsheet { get; set; }

        public short? TimesheetLayout { get; set; }
    }
}
