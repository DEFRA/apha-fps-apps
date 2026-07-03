using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for TestOrProduct VLA list operations.
    /// Enforces business rules extracted from frmTestList / fsubTest_MainList VBA logic
    /// and fps.testorproduct DDL constraints.
    /// </summary>
    public class TestListVlaService : ITestListVlaService
    {
        private readonly ITestListVlaRepository _repository;
        private readonly IMapper _mapper;

        private static readonly string[] ValidOwnerValues = { "PT", "PA", "SD", "LT" };

        public TestListVlaService(ITestListVlaRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<TestListVlaDto>> GetAllAsync(QueryParameters<string> query, int fpsYear)
        {
            ArgumentNullException.ThrowIfNull(query);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var paginationParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedAsync(paginationParams, fpsYear);
            return _mapper.Map<PaginatedResult<TestListVlaDto>>(pagedData);
        }

        public async Task<IEnumerable<TestListVlaDto>> GetAllByYearAsync(int fpsYear)
        {
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entities = await _repository.GetAllByYearAsync(fpsYear);
            return _mapper.Map<IEnumerable<TestListVlaDto>>(entities);
        }

        public async Task<TestListVlaDto?> GetByKeyAsync(string itemCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            var entity = await _repository.GetByKeyAsync(itemCode, fpsYear);
            return entity == null ? null : _mapper.Map<TestListVlaDto>(entity);
        }

        //   Guards: null check, non-empty keys, valid owner value, duplicate PK check
        public async Task<TestListVlaDto> CreateAsync(TestListVlaDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ItemCode);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            if (dto.Owner != null && !ValidOwnerValues.Contains(dto.Owner))
                throw new ArgumentException(
                    $"Owner value '{dto.Owner}' is not valid. Allowed values: {string.Join(", ", ValidOwnerValues)}.",
                    nameof(dto));

            var exists = await _repository.ExistsAsync(dto.ItemCode, dto.FpsYear);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestOrProduct VLA entry with ItemCode '{dto.ItemCode}' and FpsYear '{dto.FpsYear}' already exists.");

            var entity = _mapper.Map<TestOrProduct>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestListVlaDto>(created);
        }

        //   Guards: null check, route-key/body-key consistency, existence check, owner value validation
        public async Task<TestListVlaDto> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ItemCode);

            if (!string.Equals(itemCode, dto.ItemCode, StringComparison.OrdinalIgnoreCase) || fpsYear != dto.FpsYear)
                throw new ArgumentException(
                    "Route keys (itemCode, fpsYear) must match the DTO body keys.");

            if (dto.Owner != null && !ValidOwnerValues.Contains(dto.Owner))
                throw new ArgumentException(
                    $"Owner value '{dto.Owner}' is not valid. Allowed values: {string.Join(", ", ValidOwnerValues)}.",
                    nameof(dto));

            var existing = await _repository.GetByKeyAsync(itemCode, fpsYear);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestOrProduct VLA entry with ItemCode '{itemCode}' and FpsYear '{fpsYear}' was not found.");

            var entity = _mapper.Map<TestOrProduct>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestListVlaDto>(updated);
        }

        public async Task<bool> DeleteAsync(string itemCode, int fpsYear)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
            if (fpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(fpsYear));

            return await _repository.DeleteAsync(itemCode, fpsYear);
        }
    }
}
