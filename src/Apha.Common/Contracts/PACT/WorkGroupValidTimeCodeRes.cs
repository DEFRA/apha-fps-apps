namespace Apha.Common.Contracts.PACT
{
    public class WorkGroupValidTimeCodeRes
    {
        public string WorkGroup { get; set; } = null!;
        public string TimeCode { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public string? Manager { get; set; }
        public bool Active { get; set; }
    }
}