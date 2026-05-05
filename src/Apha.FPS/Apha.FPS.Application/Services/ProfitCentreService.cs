using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProfitCentreService : IProfitCentreService
    {
        private readonly IProfitCentreRepository _repository;
        private readonly IMapper _mapper;

        public ProfitCentreService(IProfitCentreRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ProfitCentreDto>> GetProfitCentresAsync(CancellationToken cancellationToken = default)
        {
            var result = await _repository.GetProfitCentresAsync(cancellationToken);
            return _mapper.Map<List<ProfitCentreDto>>(result);
        }
    }
}
