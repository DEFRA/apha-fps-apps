namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class WorkGroupDto
    {
        public string WorkGroupName { get; set; } = null!;
        public string? ProfitCentre { get; set; }
        public short? SendEmail { get; set; }
        public string? EmailRecipient { get; set; }
    }
}
