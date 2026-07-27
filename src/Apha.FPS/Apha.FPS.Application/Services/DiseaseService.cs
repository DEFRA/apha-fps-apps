using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly IMapper _mapper;

        public DiseaseService(IDiseaseRepository diseaseRepository, IMapper mapper)
        {
            _diseaseRepository = diseaseRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DiseaseDto>> GetAllDiseasesAsync()
        {
            var diseases = await _diseaseRepository.GetAllDiseasesAsync();
            return _mapper.Map<IEnumerable<DiseaseDto>>(diseases);
        }

        public async Task<DiseaseDto?> GetDiseaseByNameAsync(string diseaseName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(diseaseName);
            var entity = await _diseaseRepository.GetByNameAsync(diseaseName);
            return entity == null ? null : _mapper.Map<DiseaseDto>(entity);
        }

        public async Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.DiseaseName);

            if (await _diseaseRepository.ExistsAsync(dto.DiseaseName))
                throw new InvalidOperationException($"A disease with name '{dto.DiseaseName}' already exists.");

            var entity = _mapper.Map<Disease>(dto);
            var added = await _diseaseRepository.AddAsync(entity);
            return _mapper.Map<DiseaseDto>(added);
        }

        public async Task<bool> DeleteDiseaseAsync(string diseaseName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(diseaseName);
            return await _diseaseRepository.DeleteAsync(diseaseName);
        }
    }
}
