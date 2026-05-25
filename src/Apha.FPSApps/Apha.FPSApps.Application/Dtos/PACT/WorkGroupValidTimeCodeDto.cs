namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class WorkGroupValidTimeCodeDto
    {
        public string? WorkGroup { get; set; }
        public string? TimeCode { get; set; }
        public string? ParentProject { get; set; }
        public string? Manager { get; set; }
        public bool Active { get; set; }
    }
}