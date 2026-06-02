namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class WorkGroupReportEmailResultDto
    {
        public string WorkGroupName { get; set; } = string.Empty;
        public string? EmailRecipient { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
