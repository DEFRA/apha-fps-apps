namespace Apha.Common.Contracts.PACT
{
    public class TimeCodeValidReq
    {
        public string TimeCode { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public string? TestCode { get; set; }
        public string? JobCode { get; set; }
        public string? Portfolio { get; set; }
        public bool Active { get; set; }
    }
}
