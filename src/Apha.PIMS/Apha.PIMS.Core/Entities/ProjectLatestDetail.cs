namespace Apha.PIMS.Core.Entities
{
    public class ProjectLatestDetail
    {
        public string ParentProject { get; set; } = null!;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Active { get; set; }
    }
}