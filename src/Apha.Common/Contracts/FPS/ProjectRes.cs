namespace Apha.Common.Contracts.FPS
{
    public class ProjectRes
    {
        public string ParentProject { get; set; } = null!;

        public string? ProjectTitle { get; set; }

        public string? Program { get; set; }

        public decimal? BudgetCvl { get; set; }

        public short IsDefraProject { get; set; }
    }
}
