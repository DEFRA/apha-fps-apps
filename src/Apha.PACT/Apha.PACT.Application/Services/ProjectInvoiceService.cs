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
    public class ProjectInvoiceService : IProjectInvoiceService
    {
        private readonly IProjectInvoiceRepository _repository;
        private readonly IMapper _mapper;

        public ProjectInvoiceService(IProjectInvoiceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectInvoiceDto>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectInvoice> pagedData = await _repository.GetPagedProjectInvoicesAsync(parameters, parentProject);
            return _mapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData);
        }

        public async Task<decimal> GetTotalAmountAsync(string? parentProject)
            => await _repository.GetTotalAmountAsync(parentProject);

        public async Task<ProjectInvoiceDto?> GetByIdAsync(int invoiceCounter)
        {
            ProjectInvoice? entity = await _repository.GetByIdAsync(invoiceCounter);
            return entity == null ? null : _mapper.Map<ProjectInvoiceDto>(entity);
        }

        public async Task<ProjectInvoiceDto> CreateAsync(ProjectInvoiceDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.ProjectParent))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectInvoice entity = _mapper.Map<ProjectInvoice>(dto);
            ProjectInvoice created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProjectInvoiceDto>(created);
        }

        public async Task<ProjectInvoiceDto> UpdateAsync(ProjectInvoiceDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.ProjectParent))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectInvoice entity = _mapper.Map<ProjectInvoice>(dto);
            ProjectInvoice updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ProjectInvoiceDto>(updated);
        }

        public async Task<bool> DeleteAsync(int invoiceCounter)
        {
            return await _repository.DeleteAsync(invoiceCounter);
        }

        public async Task<MonthlyInvoicesPivotDto> GetMonthlyInvoicesSummaryAsync(QueryParameters<string> query)
        {
            // Parse the DataGrid JSON filter string: {"Program":"ADMIN","ParentProject":"AH"}
            string? program = null;
            string? parentProject = null;
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(query.Filter)
                           ?? new Dictionary<string, string>();
                dict.TryGetValue("Program", out program);
                dict.TryGetValue("ParentProject", out parentProject);
            }

            List<Core.Entities.MonthlyInvoicesSummary> data =
                await _repository.GetMonthlyInvoicesSummaryAsync(program, parentProject);

            List<int> months = data
                .Select(x => x.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            IEnumerable<MonthlyInvoicesSummaryDto> rows = data
                .GroupBy(x => new { x.Program, x.Parentproject })
                .Select(g => new MonthlyInvoicesSummaryDto
                {
                    Program = g.Key.Program,
                    ParentProject = g.Key.Parentproject,
                    MonthlyAmounts = g.ToDictionary(x => x.Month, x => x.Monthlyamount ?? 0m)
                });

            rows = SortPivotRows(rows, query.SortBy, query.Descending);

            return new MonthlyInvoicesPivotDto
            {
                Months = months,
                Rows = rows.ToList()
            };
        }

        private static IEnumerable<MonthlyInvoicesSummaryDto> SortPivotRows(
            IEnumerable<MonthlyInvoicesSummaryDto> rows, string? sortBy, bool descending)
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
                "program"       when descending => rows.OrderByDescending(r => r.Program).ThenByDescending(r => r.ParentProject),
                "program"                       => rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject),
                "parentproject" when descending => rows.OrderByDescending(r => r.ParentProject).ThenByDescending(r => r.Program),
                "parentproject"                 => rows.OrderBy(r => r.ParentProject).ThenBy(r => r.Program),
                _                               => rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject)
            };
        }
    }
}
