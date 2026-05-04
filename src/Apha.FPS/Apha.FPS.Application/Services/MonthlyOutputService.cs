using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class MonthlyOutputService : IMonthlyOutputService
    {
        private readonly IMonthlyOutputRepository _repository;
        private readonly IMapper _mapper;

        public MonthlyOutputService(IMonthlyOutputRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper     = mapper;
        }

        public async Task<PaginatedResult<MonthlyOutputDto>> GetByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetByProjectAsync(filter, projectCode);
            return _mapper.Map<PaginatedResult<MonthlyOutputDto>>(result);
        }

        public Task<double> GetTotalActualByProjectAsync(string projectCode)
            => _repository.GetTotalActualByProjectAsync(projectCode);

        public Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup)
            => _repository.DeleteAsync(buyer, testCode, month, workGroup);
    }
}