using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class TestSupplierService : ITestSupplierService
    {
        private readonly ITestSupplierRepository _repository;
        private readonly IMapper _mapper;

        public TestSupplierService(ITestSupplierRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestSupplierViewDto>> GetPagedByTestCodeAsync(
            QueryParameters<string> query,
            string testCode,
            bool showRejected)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetPagedByTestCodeAsync(filter, testCode, showRejected);
            return _mapper.Map<PaginatedResult<TestSupplierViewDto>>(result);
        }

        public async Task<TestRequirementDto?> GetByIdAsync(string testCode, string buyer)
        {
            var entity = await _repository.GetByIdAsync(testCode, buyer);
            return _mapper.Map<TestRequirementDto>(entity);
        }

        public async Task<TestRequirementDto> AddAsync(TestRequirementDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Converted from tlkpTestReqmt_ITrig.sql:
            // ProjectBuyerCode and TestBuyerCode must not both be null.
            await ValidateBuyerCodesAsync(dto.ProjectBuyerCode, dto.TestBuyerCode, dto.TestCode);

            var entity = _mapper.Map<TestRequirement>(dto);
            var result = await _repository.AddAsync(entity);
            return _mapper.Map<TestRequirementDto>(result);
        }

        public async Task<TestRequirementDto> UpdateAsync(TestRequirementDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Converted from tlkpTestReqmt_UTrig.sql:
            // ProjectBuyerCode and TestBuyerCode must not both be null.
            await ValidateBuyerCodesAsync(dto.ProjectBuyerCode, dto.TestBuyerCode, dto.TestCode);

            var existing = await _repository.GetByIdAsync(dto.TestCode, dto.Buyer);
            if (existing == null)
                throw new InvalidOperationException(
                    $"Test requirement with TestCode '{dto.TestCode}' and Buyer '{dto.Buyer}' was not found.");

            // If TestCode or Buyer is being changed, no MonthlyOutput records may exist.
            bool testCodeOrBuyerChanged =
                !string.Equals(existing.TestCode, dto.TestCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(existing.Buyer, dto.Buyer, StringComparison.OrdinalIgnoreCase);

            if (testCodeOrBuyerChanged)
            {
                bool hasMonthlyOutput = await _repository.MonthlyOutputExistsAsync(existing.TestCode, existing.Buyer);
                if (hasMonthlyOutput)
                    throw new InvalidOperationException(
                        $"Cannot change TestCode or Buyer: monthly output records exist for TestCode '{existing.TestCode}' and Buyer '{existing.Buyer}'.");
            }

            var entity = _mapper.Map<TestRequirement>(dto);

            if (entity.FpsYear == 0)
                entity.FpsYear = existing.FpsYear;

            var result = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestRequirementDto>(result);
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer)
        {
            // Converted from tlkpTestReqmt_Dtrig.sql:
            // No MonthlyOutput records may exist.
            bool hasMonthlyOutput = await _repository.MonthlyOutputExistsAsync(testCode, buyer);
            if (hasMonthlyOutput)
                throw new InvalidOperationException(
                    $"Cannot delete: monthly output records exist for TestCode '{testCode}' and Buyer '{buyer}'.");

            return await _repository.DeleteAsync(testCode, buyer);
        }

        public async Task<List<TestOrProductDto>> GetTestOrProductsAsync()
        {
            var items = await _repository.GetTestOrProductsAsync();
            return _mapper.Map<List<TestOrProductDto>>(items);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private async Task ValidateBuyerCodesAsync(
            string? projectBuyerCode,
            string? testBuyerCode,
            string testCode)
        {
            // Both null is invalid (trigger rule)
            if (string.IsNullOrWhiteSpace(projectBuyerCode) && string.IsNullOrWhiteSpace(testBuyerCode))
                throw new InvalidOperationException(
                    "Either ProjectBuyerCode or TestBuyerCode must be specified; both cannot be empty.");

            if (!string.IsNullOrWhiteSpace(projectBuyerCode))
            {
                bool projectExists = await _repository.ProjectExistsAsync(projectBuyerCode);
                if (!projectExists)
                    throw new InvalidOperationException(
                        $"ProjectBuyerCode '{projectBuyerCode}' does not exist in the project table.");
            }

            if (!string.IsNullOrWhiteSpace(testBuyerCode))
            {
                bool testCapabilityExists = await _repository.TestBuyerCodeExistsAsync(testCode, testBuyerCode);
                if (!testCapabilityExists)
                    throw new InvalidOperationException(
                        $"TestBuyerCode '{testBuyerCode}' is not a valid workgroup for TestCode '{testCode}' in test capability.");
            }
        }
    }
}
