using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using MapsterMapper;

namespace Apha.FPS.Application.Services
{
    public class AccountCategoryService : IAccountCategoryService
    {
        private readonly IAccountCategoryRepository _repository;
        private readonly IMapper _mapper;

        public AccountCategoryService(IAccountCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AccountCategoryDto>> GetAllAsync(QueryParameters<string> queryFilter, string? filterType = null)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(queryFilter);
            var data = await _repository.GetAllAsync(filter, filterType);
            return _mapper.Map<PaginatedResult<AccountCategoryDto>>(data);
        }

        public async Task<AccountCategoryDto?> GetByIdAsync(string accShortName)
        {
            var entity = await _repository.GetByIdAsync(accShortName);
            return _mapper.Map<AccountCategoryDto>(entity);
        }

        public async Task<AccountCategoryDto> AddAsync(AccountCategoryDto accountCategory)
        {
            ArgumentNullException.ThrowIfNull(accountCategory);
            ArgumentException.ThrowIfNullOrWhiteSpace(accountCategory.AccShortName);
            ArgumentException.ThrowIfNullOrWhiteSpace(accountCategory.AccountType);

            var exists = await _repository.ExistsByAccShortNameAsync(accountCategory.AccShortName);

            if (exists)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"An account category with AccShortName '{accountCategory.AccShortName}' already exists.",
                        "ACCOUNT_CATEGORY_ALREADY_EXISTS")
                ]);

            var entity = _mapper.Map<AccountCategory>(accountCategory);
            var result = await _repository.AddAsync(entity);
            return _mapper.Map<AccountCategoryDto>(result);
        }

        public async Task<AccountCategoryDto> UpdateAsync(string originalAccShortName, AccountCategoryDto accountCategory)
        {
            ArgumentNullException.ThrowIfNull(accountCategory);
            ArgumentException.ThrowIfNullOrWhiteSpace(originalAccShortName);
            ArgumentException.ThrowIfNullOrWhiteSpace(accountCategory.AccShortName);
            ArgumentException.ThrowIfNullOrWhiteSpace(accountCategory.AccountType);

            var existing = await _repository.GetByIdAsync(originalAccShortName);

            if (existing == null)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Account category with AccShortName '{originalAccShortName}' was not found.",
                        "ACCOUNT_CATEGORY_NOT_FOUND")
                ]);

            var entity = _mapper.Map<AccountCategory>(accountCategory);
            var result = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccountCategoryDto>(result);
        }

        public async Task<bool> DeleteAsync(string accShortName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accShortName);

            var referencedTables = await _repository.GetForeignKeyReferencesAsync(accShortName);

            if (referencedTables is { Count: > 0 })
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        "The selected account category is being used by a workgroup in budget bids or by a project in additional cost and hence it cannot be deleted.",
                        "ACCOUNT_CATEGORY_IN_USE")
                ]);
            }

            return await _repository.DeleteAsync(accShortName);
        }
    }
}
