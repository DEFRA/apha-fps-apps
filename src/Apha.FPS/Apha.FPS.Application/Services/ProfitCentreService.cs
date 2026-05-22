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

        public async Task<List<ProfitCentreDto>> GetProfitCentresAsync()
        {
            var result = await _repository.GetProfitCentresAsync();
            return _mapper.Map<List<ProfitCentreDto>>(result);
        }

        public async Task<IEnumerable<ProfitCentreDto>> GetAllProfitCentresAsync()
        {
            var views = await _repository.GetAllProfitCentresAsync();
            return _mapper.Map<IEnumerable<ProfitCentreDto>>(views);
        }

        public async Task<ProfitCentreDto?> GetProfitCentreByIdAsync(string profitCentre)
        {
            var view = await _repository.GetProfitCentreByIdAsync(profitCentre);
            return view == null ? null : _mapper.Map<ProfitCentreDto>(view);
        }

        public async Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout)
        {
            return await _repository.UpdateProfitCentreSettingsAsync(profitCentre, timesheet, outputsheet, timesheetlayout);
        }
    }
}
