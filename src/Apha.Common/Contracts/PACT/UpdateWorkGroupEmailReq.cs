namespace Apha.Common.Contracts.PACT
{
    public class UpdateWorkGroupEmailReq
    {
        public string WorkGroupName { get; set; } = null!;
        public short SendEmail { get; set; }
        public string? EmailRecipient { get; set; }
    }
}
