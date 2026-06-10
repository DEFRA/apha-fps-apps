namespace Apha.PIMS.Core.Entities
{
    public class ProjectListMilestone
    {
        public string Parentproject { get; set; } = null!;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? ProjectGroup { get; set; }
        public bool Formrequired { get; set; }
    }
}
