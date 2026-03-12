using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IDiseaseRepository _diseaseRepository;
        
        public DiseaseService(IDiseaseRepository diseaseRepository)
        {
            _diseaseRepository = diseaseRepository;
        }

        public async Task<IEnumerable<string>> GetAllDiseasesAsync()
        {
            var diseases = await _diseaseRepository.GetAllDiseasesAsync();
           return  diseases.Select(d => d.DiseaseName);           
        }
    }
}
