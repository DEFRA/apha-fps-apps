using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.DataAccess.Context
{
    public class FpsRequestContext : IFpsRequestContext
    {
        public int FpsYear { get; set; }
        public string UserEmailId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }
}
