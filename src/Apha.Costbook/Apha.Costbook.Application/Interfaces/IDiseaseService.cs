using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IDiseaseService
    {
        Task<List<DiseaseDto>> GetAllDiseasesAsync();
    }
}
