namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class BulkDeleteTimeCodeRequest
    {
        public string ParentProject { get; set; } = null!;
        public List<TimeCodeKeyItemRequest> Items { get; set; } = [];
    }

    public class TimeCodeKeyItemRequest
    {
        public string WorkGroup { get; set; } = null!;
        public string TimeCode { get; set; } = null!;
    }

    public class CopyBulkWorkGroupRequest
    {
        public string ParentProject { get; set; } = null!;
        public string SourceJobCodeId { get; set; } = null!;
        public string TargetJobCodeId { get; set; } = null!;
        public List<string> WorkGroups { get; set; } = [];
    }
}
