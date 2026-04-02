namespace Apha.Common.Contracts.PACT
{
    public class BulkDeleteTimeCodeReq
    {
        public string ParentProject { get; set; } = null!;
        public List<TimeCodeKeyItem> Items { get; set; } = [];
    }

    public class TimeCodeKeyItem
    {
        public string WorkGroup { get; set; } = null!;
        public string TimeCode { get; set; } = null!;
    }
}
