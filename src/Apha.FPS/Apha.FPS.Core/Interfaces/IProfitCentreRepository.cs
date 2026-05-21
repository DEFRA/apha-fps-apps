using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProfitCentreRepository
    {
        Task<List<ProfitCentreView>> GetProfitCentresAsync();
    }
}
