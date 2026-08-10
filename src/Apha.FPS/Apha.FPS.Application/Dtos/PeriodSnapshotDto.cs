namespace Apha.FPS.Application.Dtos
{
    public class PeriodSnapshotDto
    {
        public double EndPeriod { get; set; }
        public string? PeriodName { get; set; }
        public bool FinalSummariesRun { get; set; }
        public bool PeriodLocked { get; set; }
    }
}
