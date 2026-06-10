namespace Apha.Common.Contracts.PACT
{
    public class WorkGroupCos90sExportRes
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public byte[] Content { get; set; } = [];
    }
}
