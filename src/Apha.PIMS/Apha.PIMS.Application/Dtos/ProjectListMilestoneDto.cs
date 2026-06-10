namespace Apha.PIMS.Application.Dtos
{
    public class ProjectListMilestoneDto
    {
        public string Parentproject { get; set; } = null!;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? ProjectGroup { get; set; }
        public bool Formrequired { get; set; }
    }
}
