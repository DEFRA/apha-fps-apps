using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class TestCapabilityService : ITestCapabilityService
    {
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly ITestRequirementRepository _testReqmtRepository;
        private readonly ITestorProductRepository _testorProductRepository;
        private readonly IMonthlyOutputRepository _monthlyOutputRepository;
        private readonly IMapper _mapper;

        public TestCapabilityService(
            ITestCapabilityRepository testCapabilityRepository,
            ITestRequirementRepository testReqmtRepository,
            ITestorProductRepository testorProductRepository,
            IMonthlyOutputRepository monthlyOutputRepository,
            IMapper mapper)
        {
            _testCapabilityRepository = testCapabilityRepository;
            _testReqmtRepository = testReqmtRepository;
            _testorProductRepository = testorProductRepository;
            _monthlyOutputRepository = monthlyOutputRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedByWorkGroupAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedByTestCodeAsync(parameters, testCode);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedTestCapabilityByPortfolioAsync(QueryParameters<string> query, string? portfolio)
        {
            // Description and Unit Cost are owned by the TestorProduct master. The repository now joins
            // to that master so filtering, sorting and paging (including on Description) all happen in a
            // single database query with correct paging metadata.
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedTestCapabilityByPortfolioAsync(parameters, portfolio);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<TestCapabilityDto?> GetTestCapabilityByIdAsync(string testCode, string workGroup)
        {
            var entity = await _testCapabilityRepository.GetByIdAsync(testCode, workGroup);
            if (entity is null)
                return null;

            var dto = _mapper.Map<TestCapabilityDto>(entity);

            // Unit Cost is sourced from the TestorProduct master (testorproduct.unitpricevla),
            // not from tlkptestcapability, so the edit form always shows the master price.
            var unitPrices = await _testorProductRepository.GetUnitPricesByCodesAsync([dto.TestCode]);
            if (unitPrices != null && unitPrices.TryGetValue(dto.TestCode, out var unitPrice))
                dto.UnitCost = unitPrice;

            return dto;
        }

        public async Task<TestCapabilityDto> AddTestCapabilityAsync(TestCapabilityDto dto)
        {
            ValidateRequiredFields(dto);

            var existing = await _testCapabilityRepository.GetByIdAsync(dto.TestCode, dto.WorkGroup);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A Test Capability record with TestCode '{dto.TestCode}' and WorkGroup '{dto.WorkGroup}' already exists.");

            var entity = _mapper.Map<TestCapability>(dto);
            var created = await _testCapabilityRepository.AddAsync(entity);

            if (dto.UnitCost.HasValue)
                await _testorProductRepository.UpdateUnitPriceByCodeAsync(dto.TestCode, dto.UnitCost);

            var resultDto = _mapper.Map<TestCapabilityDto>(created);
            resultDto.UnitCost = dto.UnitCost;
            return resultDto;
        }

        public async Task<TestCapabilityDto> UpdateTestCapabilityAsync(TestCapabilityDto dto)
        {
            ValidateRequiredFields(dto);

            // WorkGroup is part of the composite key. Use the original WorkGroup (when supplied)
            // to locate the existing record; fall back to the current WorkGroup for backwards compatibility.
            var lookupWorkGroup = string.IsNullOrWhiteSpace(dto.OriginalWorkGroup)
                ? dto.WorkGroup
                : dto.OriginalWorkGroup;

            var existing = await _testCapabilityRepository.GetByIdAsync(dto.TestCode, lookupWorkGroup);
            if (existing is null)
                throw new KeyNotFoundException(
                    $"A Test Capability record with TestCode '{dto.TestCode}' and WorkGroup '{lookupWorkGroup}' was not found.");

            var hasReqmts = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestCode + lookupWorkGroup);
            if (hasReqmts)
                throw new InvalidOperationException("Cannot update, test requirements are dependant on this.");

            // Unit Cost is the master price held on testorproduct.unitpricevla, not on
            // tlkptestcapability. Persist any changed unit cost to the TestorProduct master so it
            // is reflected for every portfolio row that shares the same Test Code.
            if (dto.UnitCost.HasValue)
                await _testorProductRepository.UpdateUnitPriceByCodeAsync(dto.TestCode, dto.UnitCost.Value);

            var entity = _mapper.Map<TestCapability>(dto);
            var updated = await _testCapabilityRepository.UpdateAsync(entity, lookupWorkGroup);
            return _mapper.Map<TestCapabilityDto>(updated);
        }

            

        private static void ValidateRequiredFields(TestCapabilityDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.TestCode))
                errors.Add("Test Code is required.");

            if (string.IsNullOrWhiteSpace(dto.WorkGroup))
                errors.Add("Work Group is required.");

            if (string.IsNullOrWhiteSpace(dto.PlanPortfolio))
                errors.Add("Plan Portfolio is required.");

            if (errors.Count > 0)
                throw new ArgumentException(string.Join(" ", errors));
        }

        public async Task<bool> DeleteTestCapabilityAsync(string testCode, string workGroup)
        {
            var hasReqmts = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(testCode + workGroup);
            if (hasReqmts)
                throw new InvalidOperationException("Cannot delete, It is referenced by test requirements.");

            var hasMonthlyOutputs = await _monthlyOutputRepository.ExistsByTestCodeAndWorkGroupAsync(testCode, workGroup);
            if (hasMonthlyOutputs)
                throw new InvalidOperationException("Cannot delete, It is referenced by monthly outputs.");

            return await _testCapabilityRepository.DeleteAsync(testCode, workGroup);
        }

        public async Task<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>> GetPagedWgTestCapabilitiesWithDescriptionAsync(QueryParameters<string> query, string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("Work Group is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedWgTestCapabilitiesWithDescriptionAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>>(pagedData);
        }

        // ── Plan CrossTab ─────────────────────────────────────────────────────

        public async Task BuildTestPlanSummaryAsync()
        {
            await _testCapabilityRepository.BuildTestPlanSummaryAsync();
        }

        public async Task<TestPlanCostBreakdownDto> GetPagedTestPlanCrossTabAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _testCapabilityRepository.GetPagedTestPlanCrossTabAsync(parameters);
            return new TestPlanCostBreakdownDto
            {
                Columns = result.Columns,
                Rows = result.Rows,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
    }
}
