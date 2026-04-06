namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class BulkDeleteTimeCodeRequestDto
    {
        public string ParentProject { get; set; } = null!;
        public List<TimeCodeKeyItemDto> Items { get; set; } = [];
    }

    public class TimeCodeKeyItemDto
    {
        public string WorkGroup { get; set; } = null!;
        public string TimeCode { get; set; } = null!;
    }
}
