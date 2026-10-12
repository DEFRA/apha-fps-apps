using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using MapsterMapper;

namespace Apha.FPS.Application.Services
{
    public class TestsRequiredByRcService : ITestsRequiredByRcService
    {
        private readonly ITestsRequiredByRcRepository _repository;
        private readonly IMapper _mapper;

        public TestsRequiredByRcService(ITestsRequiredByRcRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<TestsRequiredByRcDto>> GetTestsRequiredByRcAsync(string? profitCentre)
        {
            var entities = await _repository.GetTestsRequiredByRcAsync(profitCentre);
            return _mapper.Map<List<TestsRequiredByRcDto>>(entities);
        }

        public async Task<List<TestsRequiredByRcDto>> GetTestsRequiredByRcByYearAsync(string? profitCentre, int fpsYear)
        {
            var entities = await _repository.GetTestsRequiredByRcByYearAsync(profitCentre, fpsYear);
            return _mapper.Map<List<TestsRequiredByRcDto>>(entities);
        }
    }
}
