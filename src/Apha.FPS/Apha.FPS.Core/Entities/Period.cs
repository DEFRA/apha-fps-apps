namespace Apha.FPS.Core.Entities
{
    // Maps fps.tblperiod — period status per fiscal year (PeriodName, EndPeriod, FinalSummariesRun, PeriodLocked)
    // Keyless entity: no primary key; filtered by FpsYear in repository queries
    public class Period
    {
        public string PeriodName { get; set; } = string.Empty;
        public int FpsYear { get; set; }
        public double EndPeriod { get; set; }
        public short FinalSummariesRun { get; set; }
        public short PeriodLocked { get; set; }
    }
}
