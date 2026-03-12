namespace Apha.FPS.Application.Dtos
{
    public class JobCodeDto
    {
        public string JobCodeId { get; set; } = null!;

        public string? Parentproject { get; set; }

        public string? Jobcodeworkgroup { get; set; }

        public string? Newprog { get; set; }

        public string? Type { get; set; }

        public string? Jobcodename { get; set; }

        public int? Fpscalyear { get; set; }
    }
}
