namespace Apha.PACT.Application.Dtos
{
    public class WorkGroupCos90sExportResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public byte[] Content { get; set; } = [];
    }
}
