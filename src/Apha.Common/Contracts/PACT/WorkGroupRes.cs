namespace Apha.Common.Contracts.PACT
{
    public class WorkGroupRes
    {
        public string WorkGroupName { get; set; } = null!;
        public string? ProfitCentre { get; set; }
        public short? SendEmail { get; set; }
        public string? EmailRecipient { get; set; }
    }
}
