using System.Diagnostics.CodeAnalysis;

namespace Apha.FPSApps.Application.Dtos.FPS
{
    [ExcludeFromCodeCoverage]
    public class WorkGroupViewDto
    {
        public string WorkGroupName { get; set; } = null!;

        public string ProfitCentre { get; set; } = null!;

        public string? Description { get; set; }

        public int? FpsYear { get; set; }

        public int? UserId { get; set; }

        public string? Dt2Username { get; set; }

        public string? UserEmail { get; set; }
    }
}
