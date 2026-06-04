namespace Apha.Common.Contracts.PACT
{
    public class ReleasePeriodReq
    {
        public string? PeriodName { get; set; }
        public short? FinalSummariesRun { get; set; }
        public string? SendEmail { get; set; }
    }
}