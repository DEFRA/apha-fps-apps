namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class WorkGroupCos90SExportResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public byte[] Content { get; set; } = [];
    }
}
