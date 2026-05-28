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

        public async Task<SummarisedWgTimeViewDto> GetSummarisedWorkgroupTimeSummaryAsync(
            QueryParameters<string> query,
            string workGroup)
        {
            ValidateWorkGroup(workGroup);

            var rawEntries = await _repository.GetSummarisedWorkgroupTimeAsync(workGroup);
            var entries = _mapper.Map<IEnumerable<SummarisedWgTimeEntryDto>>(rawEntries).ToList();

            var allRows = BuildSummarisedRows(entries);
            var summary = BuildSummarisedSummary(allRows);

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
                        ProjectTitle = string.IsNullOrWhiteSpace(r.ProjectTitle) ? "No description available" : r.ProjectTitle
                    })
                    .ToList()
            };
        }

        private static SummarisedWgTimeSummaryDto BuildSummarisedSummary(
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

        private static List<SummarisedWgTimeRowDto> BuildSummarisedRows(
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

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }
    }
}
