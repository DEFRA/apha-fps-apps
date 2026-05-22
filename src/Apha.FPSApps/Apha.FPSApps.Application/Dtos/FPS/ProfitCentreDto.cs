namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ProfitCentreDto
    {
        public string ProfitCentreId { get; set; } = null!;
        public string ProfitCentreName { get; set; } = null!;
        public int? Timesheet { get; set; }
        public int? Outputsheet { get; set; }
        public short? TimesheetLayout { get; set; }
    }
}
