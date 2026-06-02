namespace Apha.Common.Contracts.PACT
{
    public class ReleasePeriodReq
    {
        public string PeriodName { get; set; } = null!;
        public short FinalSummariesRun { get; set; }
    }
}