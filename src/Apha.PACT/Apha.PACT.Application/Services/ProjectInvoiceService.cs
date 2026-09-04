using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.Common.Utilities.ExcelImport;
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

        public async Task<PaginatedResult<InvoiceImportRowDto>> GetFailedInvoiceImportAsync(QueryParameters<string> query, string importedBy)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetFailedInvoiceImportAsync(parameters, importedBy);
            return _mapper.Map<PaginatedResult<InvoiceImportRowDto>>(pagedData);
        }

        public async Task<int> DeleteFailedInvoiceImportByUserAsync(string importedBy)
        {
            return await _repository.DeleteFailedInvoiceImportByUserAsync(importedBy);
        }

        public async Task<InvoiceImportResultDto> ImportInvoiceAsync(InvoiceImportDto request, string importedBy)
        {
            var validProjects = await _repository.GetValidProjectsAsync();
            var fpsYear = _repository.GetCurrentFpsYear();
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var fileName = request.FileName ?? "InvoiceImport.xlsx";

            var rowsToUpdate = new List<ProjectInvoiceStaging>();
            var stagingIdsToDelete = new List<int>();
            var passedRows = new List<ProjectInvoice>(request.Rows.Count);
            var failedRows = new List<ProjectInvoiceStaging>(request.Rows.Count);

            foreach (var source in request.Rows)
            {
                var failures = ValidateInvoiceImportRow(source, validProjects);

                var parsedMonth = ExcelParseHelper.TryParseInt(source.Month);
                var parsedAmount = ExcelParseHelper.TryParseDecimal(source.Amount);
                var parsedCostOfWork = ExcelParseHelper.TryParseDecimal(source.CostOfWork);
                var parsedWip = ExcelParseHelper.TryParseDecimal(source.Wip);
                var parsedProfitLoss = ExcelParseHelper.TryParseDecimal(source.ProfitLoss);

                if (source.Id > 0)
                {
                    var existing = await _repository.GetFailedInvoiceImportByIdAsync(source.Id, importedBy);
                    if (existing != null)
                    {
                        if (failures.Count == 0)
                        {
                            passedRows.Add(new ProjectInvoice
                            {
                                ProjectParent = source.ProjectParent!,
                                Month = parsedMonth,
                                Amount = parsedAmount,
                                CostOfWork = parsedCostOfWork,
                                Wip = parsedWip,
                                ProfitLoss = parsedProfitLoss,
                                Detail = source.Detail,
                                Type = source.Type,
                                FpsYear = fpsYear
                            });
                            stagingIdsToDelete.Add(source.Id);
                        }
                        else
                        {
                            existing.ProjectParent = source.ProjectParent;
                            existing.Month = source.Month;
                            existing.Amount = source.Amount;
                            existing.CostOfWork = source.CostOfWork;
                            existing.Wip = source.Wip;
                            existing.ProfitLoss = source.ProfitLoss;
                            existing.Detail = source.Detail;
                            existing.Type = source.Type;
                            existing.IsPassed = false;
                            existing.ValidationFailure = string.Join("\n", failures);
                            rowsToUpdate.Add(existing);
                        }
                        continue;
                    }
                }

                if (failures.Count == 0)
                {
                    passedRows.Add(new ProjectInvoice
                    {
                        ProjectParent = source.ProjectParent!,
                        Month = parsedMonth,
                        Amount = parsedAmount,
                        CostOfWork = parsedCostOfWork,
                        Wip = parsedWip,
                        ProfitLoss = parsedProfitLoss,
                        Detail = source.Detail,
                        Type = source.Type,
                        FpsYear = fpsYear
                    });
                }
                else
                {
                    failedRows.Add(new ProjectInvoiceStaging
                    {
                        ProjectParent = source.ProjectParent,
                        Month = source.Month,
                        Amount = source.Amount,
                        CostOfWork = source.CostOfWork,
                        Wip = source.Wip,
                        ProfitLoss = source.ProfitLoss,
                        Detail = source.Detail,
                        Type = source.Type,
                        Filename = fileName,
                        ImportedBy = importedBy,
                        ImportedDate = now,
                        IsPassed = false,
                        IsExported = false,
                        ValidationFailure = string.Join("\n", failures)
                    });
                }
            }

            if (rowsToUpdate.Count > 0)
            {
                await _repository.UpdateFailedInvoiceImportRecordsAsync(rowsToUpdate);
            }

            var result = await _repository.ImportInvoiceAsync(passedRows, failedRows);

            if (stagingIdsToDelete.Count > 0)
            {
                await _repository.DeleteFailedInvoiceImportByIdsAsync(stagingIdsToDelete, importedBy);
            }

            var totalPassed = result.PassedCount;
            var totalFailed = result.FailedCount + rowsToUpdate.Count;
            var totalCount = totalPassed + totalFailed;

            return new InvoiceImportResultDto
            {
                PassedCount = totalPassed,
                FailedCount = totalFailed,
                Message = $"Import completed successfully. {totalPassed} out of {totalCount} records successfully validated and is now live."
            };
        }

        private static List<string> ValidateInvoiceImportRow(InvoiceImportRowDto row, HashSet<string> validProjects)
        {
            var failures = new List<string>();

            ExcelValidationHelper.ValidateStringInSet(row.ProjectParent, validProjects, "Project Parent", failures);
            ExcelValidationHelper.ValidateRequiredDecimal(row.Amount, "Amount", failures);
            ExcelValidationHelper.ValidateMonth(row.Month, failures);
            ExcelValidationHelper.ValidateDecimal(row.CostOfWork, "Cost Of Work", failures, required: false);
            ExcelValidationHelper.ValidateDecimal(row.Wip, "WIP", failures, required: false);
            ExcelValidationHelper.ValidateDecimal(row.ProfitLoss, "Profit Loss", failures, required: false);

            return failures;
        }

        public async Task<InvoiceImportRowDto?> GetFailedInvoiceImportByIdAsync(int id, string importedBy)
        {
            var entity = await _repository.GetFailedInvoiceImportByIdAsync(id, importedBy);
            return entity == null ? null : _mapper.Map<InvoiceImportRowDto>(entity);
        }

        public async Task<bool> SaveFailedInvoiceImportAsync(int id, InvoiceImportRowDto dto, string importedBy)
        {
            var validProjects = await _repository.GetValidProjectsAsync();
            var fpsYear = _repository.GetCurrentFpsYear();

            var failures = ValidateInvoiceImportRow(dto, validProjects);

            if (failures.Count > 0)
            {
                var fieldNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Project Parent", "ProjectParent" },
                    { "Amount", "Amount" },
                    { "Month", "Month" },
                    { "Cost Of Work", "CostOfWork" },
                    { "WIP", "Wip" },
                    { "Profit Loss", "ProfitLoss" }
                };

                var validationErrors = failures.Select(failure =>
                {
                    var displayFieldName = string.Empty;

                    foreach (var fieldKey in fieldNameMap.Keys.OrderByDescending(k => k.Length))
                    {
                        if (failure.StartsWith(fieldKey, StringComparison.OrdinalIgnoreCase))
                        {
                            displayFieldName = fieldKey;
                            break;
                        }
                    }

                    var propertyName = !string.IsNullOrEmpty(displayFieldName) && fieldNameMap.TryGetValue(displayFieldName, out var mapped)
                        ? mapped
                        : string.Empty;

                    return new BusinessValidationError(failure, propertyName);
                }).ToList();

                throw new BusinessValidationErrorException(validationErrors);
            }

            // Validation passed - delete from staging and create live record
            var parsedMonth = ExcelParseHelper.TryParseInt(dto.Month);
            var parsedAmount = ExcelParseHelper.TryParseDecimal(dto.Amount);
            var parsedCostOfWork = ExcelParseHelper.TryParseDecimal(dto.CostOfWork);
            var parsedWip = ExcelParseHelper.TryParseDecimal(dto.Wip);
            var parsedProfitLoss = ExcelParseHelper.TryParseDecimal(dto.ProfitLoss);

            var newInvoice = new ProjectInvoice
            {
                ProjectParent = dto.ProjectParent!,
                Month = parsedMonth,
                Amount = parsedAmount,
                CostOfWork = parsedCostOfWork,
                Wip = parsedWip,
                ProfitLoss = parsedProfitLoss,
                Detail = dto.Detail,
                Type = dto.Type,
                FpsYear = fpsYear
            };

            await _repository.CreateAsync(newInvoice);
            await _repository.DeleteFailedInvoiceImportByIdAsync(id, importedBy);
            return true;
        }

        public async Task<bool> DeleteFailedInvoiceImportByIdAsync(int id, string importedBy)
        {
            return await _repository.DeleteFailedInvoiceImportByIdAsync(id, importedBy);
        }
    }
}
