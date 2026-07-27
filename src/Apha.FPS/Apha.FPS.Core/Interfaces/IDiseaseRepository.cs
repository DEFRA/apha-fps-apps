using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<IEnumerable<Disease>> GetAllDiseasesAsync();
        Task<Disease> AddAsync(Disease disease);
        Task<bool> DeleteAsync(string diseaseName);
        Task<bool> ExistsAsync(string diseaseName);
    }
}
