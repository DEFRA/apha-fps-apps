using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using MapsterMapper;

namespace Apha.Costbook.Application.Services
{
    public class AccountCategoryMaintenanceService : IAccountCategoryMaintenanceService
    {
        private readonly IFpsAccountCategoryRepository _repository;
        private readonly IMapper _mapper;

        public AccountCategoryMaintenanceService(IFpsAccountCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        
        public async Task<List<AccountCategoryMaintenanceDto>> GetAllForMaintenanceAsync()
        {
            var entities = await _repository.GetAllForMaintenanceAsync();
            return _mapper.Map<List<AccountCategoryMaintenanceDto>>(entities);
        }

        
        public async Task<PaginatedResult<AccountCategoryMaintenanceDto>> GetPaginatedAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetPaginatedAsync(parameters);
            return new PaginatedResult<AccountCategoryMaintenanceDto>(
                _mapper.Map<List<AccountCategoryMaintenanceDto>>(data.Data),
                _mapper.Map<PaginationDto>(data.PaginationData));
        }

        
        public async Task<AccountCategoryMaintenanceDto> UpdateCsg7GroupAsync(string accShortName, string? csg7Group, string? accountDescription)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
                throw new ArgumentException("AccShortName must not be null or empty.", nameof(accShortName));

            var existing = await _repository.GetByAccShortNameAsync(accShortName);
            if (existing is null)
                throw new KeyNotFoundException($"Account category with AccShortName '{accShortName}' was not found.");

            existing.Csg7Group = csg7Group;
            existing.AccountDescription = accountDescription;

            var updated = await _repository.UpdateAsync(existing);
            if (updated is null)
                throw new InvalidOperationException($"Failed to update account category '{accShortName}'.");

            return _mapper.Map<AccountCategoryMaintenanceDto>(updated);
        }
    }
}
