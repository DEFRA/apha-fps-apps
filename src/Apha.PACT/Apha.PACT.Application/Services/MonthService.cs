using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Interfaces;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class MonthService : IMonthService
    {
        private readonly IMonthRepository _repository;
        private readonly IMapper _mapper;

        public MonthService(IMonthRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MonthDto>> GetAllMonthsAsync()
        {
            var items = await _repository.GetAllMonthsAsync();
            return _mapper.Map<IEnumerable<MonthDto>>(items);
        }
    }
}
