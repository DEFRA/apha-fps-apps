using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class TimeCostCalcsService : ITimeCostCalcsService
    {
        private readonly ITimeCostCalcsRepository _timeCostCalcsRepository;
        private readonly IMapper _mapper;

        public TimeCostCalcsService(ITimeCostCalcsRepository timeCostCalcsRepository, IMapper mapper)
        {
            _timeCostCalcsRepository = timeCostCalcsRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TimeCostCalcsViewDto>> GetTimeCostCalcsByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            ArgumentNullException.ThrowIfNull(query);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _timeCostCalcsRepository.GetTimeCostCalcsByProjectAsync(filter, projectCode);
            return _mapper.Map<PaginatedResult<TimeCostCalcsViewDto>>(result);
        }

        public async Task<TimeCostCalcsTotalsDto> GetTotalActualByProjectAsync(string projectCode)
        {
            var (totalHours, totalCost) = await _timeCostCalcsRepository.GetTotalActualByProjectAsync(projectCode);
            return new TimeCostCalcsTotalsDto { TotalHours = totalHours, TotalCost = totalCost };
        }

        public Task<bool> DeleteTimeCostCalcsAsync(string workgroup, string jobCode, string project, double month, string staffId)
            => _timeCostCalcsRepository.DeleteAsync(workgroup, jobCode, project, month, staffId);
    }
}
