namespace Apha.Common.Contracts.PACT
{
    public class CopyInvoicesRes
    {
        public bool Success { get; set; }
        public int CopiedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
