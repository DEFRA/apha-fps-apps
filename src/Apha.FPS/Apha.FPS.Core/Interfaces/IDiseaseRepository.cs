using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<IEnumerable<Disease>> GetAllDiseasesAsync();
    }
}
