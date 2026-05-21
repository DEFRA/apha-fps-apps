using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Interfaces;
using AutoMapper;

namespace Apha.PACT.Application.Services
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

        public async Task<IEnumerable<ProfitCentreSettingsDto>> GetAllProfitCentresAsync()
        {
            var views = await _repository.GetAllProfitCentresAsync();
            return _mapper.Map<IEnumerable<ProfitCentreSettingsDto>>(views);
        }

        public async Task<ProfitCentreSettingsDto?> GetProfitCentreSettingsAsync(string profitCentre)
        {
            var view = await _repository.GetProfitCentreSettingsAsync(profitCentre);
            return view == null ? null : _mapper.Map<ProfitCentreSettingsDto>(view);
        }

        public async Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout)
        {
            return await _repository.UpdateProfitCentreSettingsAsync(profitCentre, timesheet, outputsheet, timesheetlayout);
        }
    }
}
