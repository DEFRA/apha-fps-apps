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

        public async Task<PaginatedResult<ProjectInvoiceDto>> GetPagedProjectInvoicesByMonthAsync(QueryParameters<string> query, int? month)
        {
            // Validate month if provided
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                throw new ArgumentException("Month must be between 1 and 12.", nameof(month));
            }

            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectInvoice> pagedData = await _repository.GetPagedProjectInvoicesByMonthAsync(parameters, month);
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
            // Push filter to the repository so the DB query is already filtered
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            List<Core.Entities.MonthlyInvoicesSummary> data =
                await _repository.GetMonthlyInvoicesSummaryAsync(parameters);

            // Discover all months present in filtered data (used to build columns)
            List<int> months = data
                .Select(x => x.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Group flat rows into pivot rows (must be done in-memory: dict per row)
            IEnumerable<MonthlyInvoicesSummaryDto> rows = data
                .GroupBy(x => new { x.Program, x.ParentProject })
                .Select(g => new MonthlyInvoicesSummaryDto
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

            List<MonthlyInvoicesSummaryDto> pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new MonthlyInvoicesPivotDto
            {
                Months = months,
                Rows = pagedRows,
                Pagination = new Apha.PACT.Application.Pagination.PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
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

        public async Task<CopyInvoicesResultDto> CopyInvoicesAsync(CopyInvoicesDto copyDto)
        {
            var result = new CopyInvoicesResultDto();

            // Validation
            if (copyDto.SourceMonth < 1 || copyDto.SourceMonth > 12)
            {
                result.Errors.Add("Source month must be between 1 and 12");
                return result;
            }

            if (copyDto.TargetMonth < 1 || copyDto.TargetMonth > 12)
            {
                result.Errors.Add("Target month must be between 1 and 12");
                return result;
            }

            List<ProjectInvoice> invoicesToCopy;

            // Determine copy strategy: direct invoice records, by IDs, or bulk copy
            if (copyDto.InvoiceRecords != null && copyDto.InvoiceRecords.Count > 0)
            {
                // Selective copy using provided invoice records
                invoicesToCopy = copyDto.InvoiceRecords
                    .Select(dto => _mapper.Map<ProjectInvoice>(dto))
                    .ToList();
            }
            else if (copyDto.InvoiceIds != null && copyDto.InvoiceIds.Count > 0)
            {
                // Selective copy using provided invoice IDs
                invoicesToCopy = await _repository.GetInvoicesByIdsAsync(copyDto.InvoiceIds);

                if (invoicesToCopy == null || invoicesToCopy.Count == 0)
                {
                    result.Errors.Add("No invoices found with the provided IDs");
                    return result;
                }
            }
            else
            {
                // Bulk copy: all invoices from source month
                invoicesToCopy = await _repository.GetInvoicesByMonthAsync(copyDto.SourceMonth);

                if (invoicesToCopy == null || invoicesToCopy.Count == 0)
                {
                    result.Errors.Add($"No invoices found for source month {copyDto.SourceMonth}");
                    return result;
                }
            }

            // Copy invoices to target month using bulk insert
            var newInvoices = invoicesToCopy.Select(sourceInvoice => new ProjectInvoice
            {
                ProjectParent = sourceInvoice.ProjectParent,
                Month = copyDto.TargetMonth,
                Amount = sourceInvoice.Amount,
                CostOfWork = sourceInvoice.CostOfWork,
                Wip = sourceInvoice.Wip,
                ProfitLoss = sourceInvoice.ProfitLoss,
                Detail = sourceInvoice.Detail,
                FpsYear = sourceInvoice.FpsYear
            }).ToList();

            int inserted = await _repository.CreateBulkAsync(newInvoices);
            result.CopiedCount = newInvoices.Count;


            result.Success = result.FailedCount == 0;
            result.Message = result.Success
                ? $"Successfully copied {result.CopiedCount} invoice(s) from month {copyDto.SourceMonth} to month {copyDto.TargetMonth}"
                : $"Copied {result.CopiedCount} invoice(s) with {result.FailedCount} failure(s)";

            return result;
        }
    }
}
