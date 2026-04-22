namespace Apha.PACT.Application.Dtos
{
    public class TimeCodeValidDto
    {
        public string TimeCode { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public string? TestCode { get; set; }
        public string? JobCode { get; set; }
        public string? Portfolio { get; set; }
        public bool Active { get; set; }
        public int FpsYear { get; set; }
    }
}
