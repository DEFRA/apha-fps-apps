using System.Diagnostics.CodeAnalysis;

namespace Apha.FPSApps.Application.Dtos.FPS
{
    [ExcludeFromCodeCoverage]
    public class BidViewDto
    {
        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }

        public int FpsYear { get; set; }

        public int? UserId { get; set; }

        public string? Dt2Username { get; set; }

        public string? UserEmail { get; set; }
    }
}
