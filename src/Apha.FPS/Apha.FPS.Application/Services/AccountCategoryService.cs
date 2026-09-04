using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

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
                throw new InvalidOperationException(
                    $"An account category with AccShortName '{accountCategory.AccShortName}' already exists.");

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
                throw new InvalidOperationException(
                    $"Account category with AccShortName '{originalAccShortName}' was not found.");

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
                throw new InvalidOperationException("The selected record is being used on another page and cannot be deleted.");
            }

            return await _repository.DeleteAsync(accShortName);
        }
    }
}
