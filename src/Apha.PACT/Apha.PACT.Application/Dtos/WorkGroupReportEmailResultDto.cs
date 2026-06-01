namespace Apha.PACT.Application.Dtos
{
    public class WorkGroupReportEmailResultDto
    {
        public string WorkGroupName { get; set; } = string.Empty;
        public string? EmailRecipient { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
