using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Interfaces;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class CalenderMonthService : ICalenderMonthService
    {
        private readonly ICalenderMonthRepository _repository;
        private readonly IMapper _mapper;

        public CalenderMonthService(ICalenderMonthRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CalenderMonthDto>> GetAllCalenderMonthsAsync()
        {
            var items = await _repository.GetAllCalenderMonthsAsync();
            return _mapper.Map<IEnumerable<CalenderMonthDto>>(items);
        }
    }
}
