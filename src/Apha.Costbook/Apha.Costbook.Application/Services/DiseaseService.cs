using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Application.Dtos;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IDiseaseRepository _repo;
        private readonly IMapper _mapper;

        public DiseaseService(IDiseaseRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<DiseaseDto>> GetAllDiseasesAsync()
        {
            var diseases = await _repo.GetAllDiseasesAsync();
            return _mapper.Map<List<DiseaseDto>>(diseases);
        }
    }
}
