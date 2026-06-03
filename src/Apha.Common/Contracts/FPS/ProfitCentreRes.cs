namespace Apha.Common.Contracts.FPS
{
   
    public class ProfitCentreRes
    {
        public string ProfitCentreId { get; set; } = null!;
        public string ProfitCentreName { get; set; } = null!;
        public string? Division { get; set; }
        public decimal? ContTarget { get; set; }
        public string? ProfitCentreHead { get; set; }
        public int? DivisionId { get; set; }
        public string? EmailRecipient { get; set; }
        public int? Timesheet { get; set; }
        public int? Outputsheet { get; set; }
        public short? TimesheetLayout { get; set; }
    }
}
