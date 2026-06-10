namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class ReleasePeriodDto
    {
        public string PeriodName { get; set; } = null!;
        public string? PeriodType { get; set; }
        public double? StartPeriod { get; set; }
        public double? EndPeriod { get; set; }
        public short? FinalSummariesRun { get; set; }
        public short PeriodLocked { get; set; }
    }
}