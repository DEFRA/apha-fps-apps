namespace Apha.Common.Contracts.FPS
{
    public class ProfitCentreRes
    {
        public string ProfitCentreId { get; set; } = null!;
        public string ProfitCentreName { get; set; } = null!;
        public int? Timesheet { get; set; }
        public int? Outputsheet { get; set; }
        public short? TimesheetLayout { get; set; }
    }
}
