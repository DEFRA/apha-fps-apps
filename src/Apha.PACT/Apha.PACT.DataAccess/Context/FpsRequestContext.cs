using Apha.PACT.Core.Interfaces;

namespace Apha.PACT.DataAccess.Context
{
    public class FpsRequestContext : IFpsRequestContext
    {
        public int FpsYear { get; set; }
        public string UserEmailId { get; set; } = string.Empty;
    }
}
