using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class MonthlyOutputCalcsService : IMonthlyOutputCalcsService
    {
        private readonly IMonthlyOutputCalcsRepository _repository;
        private readonly IMapper _mapper;

        public MonthlyOutputCalcsService(IMonthlyOutputCalcsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper     = mapper;
        }

        public async Task<PaginatedResult<MonthlyOutputCalcsViewDto>> GetByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetByProjectAsync(filter, projectCode);
            return _mapper.Map<PaginatedResult<MonthlyOutputCalcsViewDto>>(result);
        }

        public async Task<MonthlyOutputCalcsTotalsDto> GetTotalActualByProjectAsync(string projectCode)
        {
            var (totalVolume, totalCost) = await _repository.GetTotalActualByProjectAsync(projectCode);
            return new MonthlyOutputCalcsTotalsDto { TotalVolume = totalVolume, TotalCost = totalCost };
        }

        public Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup)
            => _repository.DeleteAsync(buyer, testCode, month, workGroup);
    }
}