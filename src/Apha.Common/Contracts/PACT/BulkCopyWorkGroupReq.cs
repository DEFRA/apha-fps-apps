namespace Apha.Common.Contracts.PACT
{
    public class BulkCopyWorkGroupReq
    {
        public string ParentProject { get; set; } = null!;
        public string SourceJobCode { get; set; } = null!;
        public string TargetJobCode { get; set; } = null!;
        public List<string> WorkGroups { get; set; } = [];
    }
}
