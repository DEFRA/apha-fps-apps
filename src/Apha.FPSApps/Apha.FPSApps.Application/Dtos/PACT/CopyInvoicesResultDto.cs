namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class CopyInvoicesResultDto
    {
        public bool Success { get; set; }
        public int CopiedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
