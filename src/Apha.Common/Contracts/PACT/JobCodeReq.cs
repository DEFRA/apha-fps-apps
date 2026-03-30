namespace Apha.Common.Contracts.PACT
{
    public class JobCodeReq
    {
        public string JobCodeId { get; set; } = null!;
        public string? ParentProject { get; set; }
        public string? JobCodeWorkGroup { get; set; }
        public string? Type { get; set; }
        public string? JobCodeName { get; set; }
    }
}
