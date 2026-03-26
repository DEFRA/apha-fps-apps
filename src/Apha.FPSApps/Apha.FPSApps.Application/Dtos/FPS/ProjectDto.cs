namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ProjectDto
    {
        public string ParentProject { get; set; } = null!;

        public string? ProjectTitle { get; set; }

        public string? Program { get; set; }

        public decimal? BudgetCvl { get; set; }

        public short IsDefraProject { get; set; }
    }
}
