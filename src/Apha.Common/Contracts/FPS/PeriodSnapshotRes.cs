namespace Apha.Common.Contracts.FPS
{
    public class PeriodSnapshotRes
    {
        public double EndPeriod { get; set; }
        public string? PeriodName { get; set; }
        public bool FinalSummariesRun { get; set; }
        public bool PeriodLocked { get; set; }
    }
}
