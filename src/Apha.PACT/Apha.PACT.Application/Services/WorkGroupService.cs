using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IWorkGroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupService(IWorkGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync()
        {
            var items = await _repository.GetAllWorkGroupsAsync();
            return _mapper.Map<IEnumerable<WorkGroupDto>>(items);
        }

        public async Task<List<string>> GetAllWorkGroupNamesAsync()
            => await _repository.GetAllWorkGroupNamesAsync();


        public async Task<List<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var views = await _repository.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
            return _mapper.Map<List<WorkGroupViewDto>>(views);
        }

        public async Task<PaginatedResult<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            QueryParameters<string> query, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupsByProfitCentreForBudgetPagedAsync(parameters, profitCentre);
            return _mapper.Map<PaginatedResult<WorkGroupViewDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupTimeCodeDto>> GetWorkGroupTimeCodeAsync(QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupTimeCodeAsync(parameters, workGroup, monthNumber);
            return _mapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupValidTimeCodeDto>> GetWorkGroupValidTimeCodeAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupValidTimeCodeAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData);
        }

        public async Task<WgSummarisedStaffTimeUsageDto> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string staffName)
        {
            ValidateStaffName(staffName);

            var rawEntries = await _repository.GetWgSummarisedStaffTimeUsageAsync(staffName);
            var entries = _mapper.Map<IEnumerable<WgSummarisedStaffTimeUsageEntryDto>>(rawEntries);

            // Derive HrsPaid: sum across all distinct people in the work group
            var hrsPaid = entries
                .GroupBy(e => e.Name)
                .Select(g => g.First())
                .Sum(e => e.HrsPaid ?? 0);

            var standardHoursPerMonth = hrsPaid > 0 ? hrsPaid / 12.0 : 0;

            // Build ALL rows first — summary must reflect the full dataset, not just the current page
            var allRows = BuildRows(entries);
            var summary = BuildSummary(allRows, standardHoursPerMonth);

            // Apply sort before paging so the requested order is respected
            allRows = ApplySortToWgStaffTimeRows(allRows, query.SortBy, query.Descending);

            // Paginate rows after summary is computed
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            var totalRecords = allRows.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new WgSummarisedStaffTimeUsageDto
            {
                Rows = pagedRows,
                Summary = summary,
                HrsPaid = hrsPaid,
                JobTitleLookup = entries
                    .Where(r => !string.IsNullOrWhiteSpace(r.JobCode))
                    .DistinctBy(r => r.JobCode)
                    .Select(r => new JobTitleLookupItem
                    {
                        JobCode = r.JobCode!,
                        JobTitle = string.IsNullOrWhiteSpace(r.JobTitle) ? string.Empty : r.JobTitle
                    })
                    .ToList(),
                Pagination = new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<SummarisedWgTimeViewDto> GetSummarisedWorkgroupTimeSummaryAsync(
           QueryParameters<string> query,
           string workGroup)
        {
            ValidateWorkGroup(workGroup);

            var rawEntries = await _repository.GetSummarisedWorkgroupTimeAsync(workGroup);
            var entries = _mapper.Map<IEnumerable<SummarisedWgTimeEntryDto>>(rawEntries).ToList();

            var allRows = BuildWgSummarisedTimeRows(entries);
            var summary = BuildWgSummarisedTimeSummary(allRows);

            // Apply sort before paging so the requested order is respected
            allRows = ApplySortToWgSummarisedTimeRows(allRows, query.SortBy, query.Descending);

            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            var totalRecords = allRows.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new SummarisedWgTimeViewDto
            {
                Rows = pagedRows,
                Summary = summary,
                Pagination = new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                },
                ProjectTitleLookup = entries
                    .Where(r => !string.IsNullOrWhiteSpace(r.ParentProject))
                    .DistinctBy(r => r.ParentProject)
                    .Select(r => new ProjectTitleLookupItem
                    {
                        ParentProject = r.ParentProject!,
                        ProjectTitle = string.IsNullOrWhiteSpace(r.ProjectTitle) ? "" : r.ProjectTitle
                    })
                    .ToList()
            };
        }

        public async Task<PaginatedResult<WorkGroupDto>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupsByProfitCentreAsync(parameters, profitCentre);
            return _mapper.Map<PaginatedResult<WorkGroupDto>>(pagedData);
        }

        public async Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            return await _repository.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        public async Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            return await _repository.SetSendEmailForAllWorkGroupsAsync(flag);
        }

        public async Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient)
        {
            return await _repository.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        private static List<WgSummarisedStaffTimeUsageRowDto> BuildRows(
            IEnumerable<WgSummarisedStaffTimeUsageEntryDto> staffTimeUsageEntries)
        {
            return staffTimeUsageEntries
                .GroupBy(e => new { e.ParentProject, e.JobCode })
                .Select(g =>
                {
                    double HoursForMonth(string monthName) =>
                        g.Where(e => e.MonthName!.Equals(monthName, StringComparison.CurrentCultureIgnoreCase)).Sum(e => e.TotalTime ?? 0);

                    return new WgSummarisedStaffTimeUsageRowDto
                    {
                        ParentProject = g.Key.ParentProject,
                        JobCode = g.Key.JobCode,
                        JobTitle = string.IsNullOrWhiteSpace(g.First().JobTitle) ? string.Empty : g.First().JobTitle,
                        April = HoursForMonth("April"),
                        May = HoursForMonth("May"),
                        June = HoursForMonth("June"),
                        July = HoursForMonth("July"),
                        August = HoursForMonth("August"),
                        September = HoursForMonth("September"),
                        October = HoursForMonth("October"),
                        November = HoursForMonth("November"),
                        December = HoursForMonth("December"),
                        January = HoursForMonth("January"),
                        February = HoursForMonth("February"),
                        March = HoursForMonth("March"),
                        TotalTime = g.Sum(e => e.TotalTime ?? 0),
                        TotalCost = g.Sum(e => e.TotalCost ?? 0)
                    };
                })
                .OrderBy(r => r.ParentProject)
                .ThenBy(r => r.JobCode)
                .ToList();
        }

        /// <summary>
        /// Builds the three-row footer that appeared at the botton.
        /// </summary>
        private static WgSummarisedStaffTimeUsageSummaryDto BuildSummary(
            IReadOnlyList<WgSummarisedStaffTimeUsageRowDto> rows, double standardHoursPerMonth)
        {
            var totalHoursApril = rows.Sum(r => r.April);
            var totalHoursMay = rows.Sum(r => r.May);
            var totalHoursJune = rows.Sum(r => r.June);
            var totalHoursJuly = rows.Sum(r => r.July);
            var totalHoursAugust = rows.Sum(r => r.August);
            var totalHoursSeptember = rows.Sum(r => r.September);
            var totalHoursOctober = rows.Sum(r => r.October);
            var totalHoursNovember = rows.Sum(r => r.November);
            var totalHoursDecember = rows.Sum(r => r.December);
            var totalHoursJanuary = rows.Sum(r => r.January);
            var totalHoursFebruary = rows.Sum(r => r.February);
            var totalHoursMarch = rows.Sum(r => r.March);
            var grandTotalTime = rows.Sum(r => r.TotalTime);

            // Returns the standard hours allowance for a month
            double StandardHoursFor(double totalHoursInMonth)
            {
                return totalHoursInMonth == 0 ? 0 : standardHoursPerMonth;
            }

            // Percentage of recorded hours against the standard hours allowance for a single month, rounded to one decimal place;
            double PercentAllocated(double totalHoursInMonth, double standardHours)
            {
                return standardHours == 0 ? 0 : Math.Round(totalHoursInMonth / standardHours * 100, 1);
            }

            return new WgSummarisedStaffTimeUsageSummaryDto
            {
                TotalApril = totalHoursApril,
                TotalMay = totalHoursMay,
                TotalJune = totalHoursJune,
                TotalJuly = totalHoursJuly,
                TotalAugust = totalHoursAugust,
                TotalSeptember = totalHoursSeptember,
                TotalOctober = totalHoursOctober,
                TotalNovember = totalHoursNovember,
                TotalDecember = totalHoursDecember,
                TotalJanuary = totalHoursJanuary,
                TotalFebruary = totalHoursFebruary,
                TotalMarch = totalHoursMarch,
                GrandTotalTime = grandTotalTime,
                GrandTotalCost = rows.Sum(r => r.TotalCost),
                StandardHoursPerMonth = standardHoursPerMonth,

                // Sum of the standard hours allowance for each month that had recorded activity;
                TotalStandardHours =
                    StandardHoursFor(totalHoursApril)     + StandardHoursFor(totalHoursMay)      +
                    StandardHoursFor(totalHoursJune)      + StandardHoursFor(totalHoursJuly)     +
                    StandardHoursFor(totalHoursAugust)    + StandardHoursFor(totalHoursSeptember)+
                    StandardHoursFor(totalHoursOctober)   + StandardHoursFor(totalHoursNovember) +
                    StandardHoursFor(totalHoursDecember)  + StandardHoursFor(totalHoursJanuary)  +
                    StandardHoursFor(totalHoursFebruary)  + StandardHoursFor(totalHoursMarch),

                // Percentage of total recorded hours against the sum of standard hours for all months that had activity;
                GrandTotalPercentAllocated = (
                    StandardHoursFor(totalHoursApril)      + StandardHoursFor(totalHoursMay)       +
                    StandardHoursFor(totalHoursJune)       + StandardHoursFor(totalHoursJuly)      +
                    StandardHoursFor(totalHoursAugust)     + StandardHoursFor(totalHoursSeptember) +
                    StandardHoursFor(totalHoursOctober)    + StandardHoursFor(totalHoursNovember)  +
                    StandardHoursFor(totalHoursDecember)   + StandardHoursFor(totalHoursJanuary)   +
                    StandardHoursFor(totalHoursFebruary)   + StandardHoursFor(totalHoursMarch)) > 0
                        ? Math.Round(grandTotalTime /
                            (StandardHoursFor(totalHoursApril)      + StandardHoursFor(totalHoursMay)       +
                             StandardHoursFor(totalHoursJune)       + StandardHoursFor(totalHoursJuly)      +
                             StandardHoursFor(totalHoursAugust)     + StandardHoursFor(totalHoursSeptember) +
                             StandardHoursFor(totalHoursOctober)    + StandardHoursFor(totalHoursNovember)  +
                             StandardHoursFor(totalHoursDecember)   + StandardHoursFor(totalHoursJanuary)   +
                             StandardHoursFor(totalHoursFebruary)   + StandardHoursFor(totalHoursMarch)) * 100, 1)
                        : 0,

                PercentAllocatedApril = PercentAllocated(totalHoursApril, StandardHoursFor(totalHoursApril)),
                PercentAllocatedMay = PercentAllocated(totalHoursMay, StandardHoursFor(totalHoursMay)),
                PercentAllocatedJune = PercentAllocated(totalHoursJune, StandardHoursFor(totalHoursJune)),
                PercentAllocatedJuly = PercentAllocated(totalHoursJuly, StandardHoursFor(totalHoursJuly)),
                PercentAllocatedAugust = PercentAllocated(totalHoursAugust, StandardHoursFor(totalHoursAugust)),
                PercentAllocatedSeptember = PercentAllocated(totalHoursSeptember, StandardHoursFor(totalHoursSeptember)),
                PercentAllocatedOctober = PercentAllocated(totalHoursOctober, StandardHoursFor(totalHoursOctober)),
                PercentAllocatedNovember = PercentAllocated(totalHoursNovember, StandardHoursFor(totalHoursNovember)),
                PercentAllocatedDecember = PercentAllocated(totalHoursDecember, StandardHoursFor(totalHoursDecember)),
                PercentAllocatedJanuary = PercentAllocated(totalHoursJanuary, StandardHoursFor(totalHoursJanuary)),
                PercentAllocatedFebruary = PercentAllocated(totalHoursFebruary, StandardHoursFor(totalHoursFebruary)),
                PercentAllocatedMarch = PercentAllocated(totalHoursMarch, StandardHoursFor(totalHoursMarch))
            };
        }

        private static void ValidateStaffName(string staffName)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(staffName))
                errors.Add(new BusinessValidationError("Staff Name is required", "STAFFNane_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }

        private static List<SummarisedWgTimeRowDto> ApplySortToWgSummarisedTimeRows(
            List<SummarisedWgTimeRowDto> rows,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows;

            Func<SummarisedWgTimeRowDto, object> keySelector = sortBy switch
            {
                nameof(SummarisedWgTimeRowDto.ParentProject) => r => (object)(r.ParentProject ?? string.Empty),
                nameof(SummarisedWgTimeRowDto.April) => r => r.April,
                nameof(SummarisedWgTimeRowDto.May) => r => r.May,
                nameof(SummarisedWgTimeRowDto.June) => r => r.June,
                nameof(SummarisedWgTimeRowDto.July) => r => r.July,
                nameof(SummarisedWgTimeRowDto.August) => r => r.August,
                nameof(SummarisedWgTimeRowDto.September) => r => r.September,
                nameof(SummarisedWgTimeRowDto.October) => r => r.October,
                nameof(SummarisedWgTimeRowDto.November) => r => r.November,
                nameof(SummarisedWgTimeRowDto.December) => r => r.December,
                nameof(SummarisedWgTimeRowDto.January) => r => r.January,
                nameof(SummarisedWgTimeRowDto.February) => r => r.February,
                nameof(SummarisedWgTimeRowDto.March) => r => r.March,
                nameof(SummarisedWgTimeRowDto.TotalTime) => r => r.TotalTime,
                nameof(SummarisedWgTimeRowDto.TotalCost) => r => r.TotalCost,
                _ => r => (object)(r.ParentProject ?? string.Empty)
            };

            return descending
                ? rows.OrderByDescending(keySelector).ToList()
                : rows.OrderBy(keySelector).ToList();
        }

        private static List<WgSummarisedStaffTimeUsageRowDto> ApplySortToWgStaffTimeRows(
            List<WgSummarisedStaffTimeUsageRowDto> rows,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows;

            Func<WgSummarisedStaffTimeUsageRowDto, object> keySelector = sortBy switch
            {
                nameof(WgSummarisedStaffTimeUsageRowDto.ParentProject) => r => (object)(r.ParentProject ?? string.Empty),
                nameof(WgSummarisedStaffTimeUsageRowDto.JobCode) => r => (object)(r.JobCode ?? string.Empty),
                nameof(WgSummarisedStaffTimeUsageRowDto.April) => r => r.April,
                nameof(WgSummarisedStaffTimeUsageRowDto.May) => r => r.May,
                nameof(WgSummarisedStaffTimeUsageRowDto.June) => r => r.June,
                nameof(WgSummarisedStaffTimeUsageRowDto.July) => r => r.July,
                nameof(WgSummarisedStaffTimeUsageRowDto.August) => r => r.August,
                nameof(WgSummarisedStaffTimeUsageRowDto.September) => r => r.September,
                nameof(WgSummarisedStaffTimeUsageRowDto.October) => r => r.October,
                nameof(WgSummarisedStaffTimeUsageRowDto.November) => r => r.November,
                nameof(WgSummarisedStaffTimeUsageRowDto.December) => r => r.December,
                nameof(WgSummarisedStaffTimeUsageRowDto.January) => r => r.January,
                nameof(WgSummarisedStaffTimeUsageRowDto.February) => r => r.February,
                nameof(WgSummarisedStaffTimeUsageRowDto.March) => r => r.March,
                nameof(WgSummarisedStaffTimeUsageRowDto.TotalTime) => r => r.TotalTime,
                nameof(WgSummarisedStaffTimeUsageRowDto.TotalCost) => r => r.TotalCost,
                _ => r => (object)(r.ParentProject ?? string.Empty)
            };

            return descending
                ? rows.OrderByDescending(keySelector).ToList()
                : rows.OrderBy(keySelector).ToList();
        }

        private static List<SummarisedWgTimeRowDto> BuildWgSummarisedTimeRows(
           IEnumerable<SummarisedWgTimeEntryDto> entries)
        {
            return entries
                .GroupBy(e => e.ParentProject)
                .Select(g =>
                {
                    double HoursForMonth(string monthName) =>
                        g.Where(e => e.MonthName!.Equals(monthName, StringComparison.CurrentCultureIgnoreCase))
                         .Sum(e => e.TotalTime.GetValueOrDefault());

                    return new SummarisedWgTimeRowDto
                    {
                        ParentProject = g.Key,
                        April = HoursForMonth("April"),
                        May = HoursForMonth("May"),
                        June = HoursForMonth("June"),
                        July = HoursForMonth("July"),
                        August = HoursForMonth("August"),
                        September = HoursForMonth("September"),
                        October = HoursForMonth("October"),
                        November = HoursForMonth("November"),
                        December = HoursForMonth("December"),
                        January = HoursForMonth("January"),
                        February = HoursForMonth("February"),
                        March = HoursForMonth("March"),
                        TotalTime = g.Sum(e => e.TotalTime.GetValueOrDefault()),
                        TotalCost = g.Sum(e => e.TotalCost.GetValueOrDefault())
                    };
                })
                .OrderBy(r => r.ParentProject)
                .ToList();
        }

        private static SummarisedWgTimeSummaryDto BuildWgSummarisedTimeSummary(
            IReadOnlyList<SummarisedWgTimeRowDto> rows)
        {
            return new SummarisedWgTimeSummaryDto
            {
                TotalApril = rows.Sum(r => r.April),
                TotalMay = rows.Sum(r => r.May),
                TotalJune = rows.Sum(r => r.June),
                TotalJuly = rows.Sum(r => r.July),
                TotalAugust = rows.Sum(r => r.August),
                TotalSeptember = rows.Sum(r => r.September),
                TotalOctober = rows.Sum(r => r.October),
                TotalNovember = rows.Sum(r => r.November),
                TotalDecember = rows.Sum(r => r.December),
                TotalJanuary = rows.Sum(r => r.January),
                TotalFebruary = rows.Sum(r => r.February),
                TotalMarch = rows.Sum(r => r.March),
                GrandTotalTime = rows.Sum(r => r.TotalTime),
                GrandTotalCost = rows.Sum(r => r.TotalCost)
            };
        }
    }
}
