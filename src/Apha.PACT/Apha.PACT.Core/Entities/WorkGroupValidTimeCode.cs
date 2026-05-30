namespace Apha.PACT.Core.Entities
{
    public class WorkGroupValidTimeCode
    {
        public string WorkGroup { get; set; } = null!;
        public string? Manager { get; set; }
        public string TimeCode { get; set; } = null!;
        public bool Active { get; set; }
        public string ParentProject { get; set; } = null!;
    }
}