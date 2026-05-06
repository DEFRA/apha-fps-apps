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
    public class ProjectSubContractService : IProjectSubContractService
    {
        private readonly IProjectSubContractRepository _repository;
        private readonly IMapper _mapper;

        public ProjectSubContractService(IProjectSubContractRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectSubContractDto>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectSubContract> pagedData = await _repository.GetPagedProjectSubContractsAsync(parameters, project);
            return _mapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData);
        }

        public async Task<decimal> GetTotalAmountAsync(string? project)
            => await _repository.GetTotalAmountAsync(project);

        public async Task<PaginatedResult<ProjectSubContractDto>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectSubContract> pagedData = await _repository.GetFpsProjectSubContractsAsync(parameters, project);
            return _mapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData);
        }

        public async Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project)
            => await _repository.GetFpsProjectSubContractTotalAmountAsync(project);

        public async Task<ProjectSubContractDto?> GetByIdAsync(int subContCounter)
        {
            ProjectSubContract? entity = await _repository.GetByIdAsync(subContCounter);
            return entity == null ? null : _mapper.Map<ProjectSubContractDto>(entity);
        }

        public async Task<ProjectSubContractDto> CreateAsync(ProjectSubContractDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectSubContract entity = _mapper.Map<ProjectSubContract>(dto);
            ProjectSubContract created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProjectSubContractDto>(created);
        }

        public async Task<ProjectSubContractDto> UpdateAsync(ProjectSubContractDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectSubContract entity = _mapper.Map<ProjectSubContract>(dto);
            ProjectSubContract updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ProjectSubContractDto>(updated);
        }

        public async Task<bool> DeleteAsync(int subContCounter)
        {
            return await _repository.DeleteAsync(subContCounter);
        }

        public async Task<MonthlySubContractsPivotDto> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query)
        {
            // Push filter to the repository so the DB query is already filtered
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            
          var data = await _repository.GetMonthlySubContractsSummaryAsync(parameters);

            // Discover all months present in filtered data (used to build columns)
            List<int> months = data
                .Select(x => x.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Group flat rows into pivot rows (must be done in-memory: dict per row)
            IEnumerable<MonthlySubContractsSummaryDto> rows = data
                .GroupBy(x => new { x.Program, x.ParentProject })
                .Select(g => new MonthlySubContractsSummaryDto
                {
                    Program = g.Key.Program,
                    ParentProject = g.Key.ParentProject,
                    MonthlyAmounts = g.ToDictionary(x => x.Month, x => x.MonthlyAmount ?? 0m)
                });

            // Sort grouped pivot rows (including dynamic month columns M1..M12)
            rows = SortPivotRows(rows, query.SortBy, query.Descending);

            // Paginate grouped rows in-memory
            var allRows = rows.ToList();
            int totalRecords = allRows.Count;
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize < 1 ? 10 : query.PageSize;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            List<MonthlySubContractsSummaryDto> pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new MonthlySubContractsPivotDto
            {
                Months = months,
                Rows = pagedRows,
                Pagination = new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            };
        }

        private static IEnumerable<MonthlySubContractsSummaryDto> SortPivotRows(
            IEnumerable<MonthlySubContractsSummaryDto> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject);

            // Dynamic month column: PropertyName is "M1" … "M12"
            // Parse the month number and sort by the corresponding amount value
            if (sortBy.StartsWith("M", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(sortBy[1..], out int month)
                && month is >= 1 and <= 12)
            {
                return descending
                    ? rows.OrderByDescending(r => r.MonthlyAmounts.GetValueOrDefault(month))
                          .ThenBy(r => r.Program)
                    : rows.OrderBy(r => r.MonthlyAmounts.GetValueOrDefault(month))
                          .ThenBy(r => r.Program);
            }

            return sortBy.ToLower() switch
            {
                "program" when descending => rows.OrderByDescending(r => r.Program).ThenByDescending(r => r.ParentProject),
                "program" => rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject),
                "parentproject" when descending => rows.OrderByDescending(r => r.ParentProject).ThenByDescending(r => r.Program),
                "parentproject" => rows.OrderBy(r => r.ParentProject).ThenBy(r => r.Program),
                _ => rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject)
            };
        }
    }
}
