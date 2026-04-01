namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class JobCodeDto
    {
        public string JobCodeId { get; set; } = null!;
        public string? ParentProject { get; set; }
        public string? JobCodeWorkGroup { get; set; }
        public string? Type { get; set; }
        public string? JobCodeName { get; set; }
    }
}
