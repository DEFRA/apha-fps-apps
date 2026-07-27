using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IDiseaseService
    {
        Task<IEnumerable<DiseaseDto>> GetAllDiseasesAsync();
        Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto dto);
        Task<bool> DeleteDiseaseAsync(string diseaseName);
    }
}
