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

        public async Task<SummarisedWgTimePivotDto> GetSummarisedWorkgroupTimeSummaryAsync(
            QueryParameters<string> query,
            string? workGroup)
        {
            // Get all data from repository (normalized format: one row per project per month)
            var data = await _repository.GetSummarisedWorkgroupTimeAsync(workGroup);

            // Group by project and pivot the months
            var pivotedData = data
                .GroupBy(x => new
                {
                    x.WorkGroup,
                    x.ParentProject
                })
                .Select(group =>
                {
                    var dto = new SummarisedWgTimeDto
                    {
                        WorkGroup = group.Key.WorkGroup,
                        ParentProject = group.Key.ParentProject,
                        ProjectTitle = group.FirstOrDefault()?.Name // Assuming ProjectTitle is in Name field
                    };

                    // Pivot months - sum time for each month
                    foreach (var item in group)
                    {
                        switch (item.MonthName?.ToLower())
                        {
                            case "april":
                                dto.April = (dto.April ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "may":
                                dto.May = (dto.May ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "june":
                                dto.June = (dto.June ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "july":
                                dto.July = (dto.July ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "august":
                                dto.August = (dto.August ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "september":
                                dto.September = (dto.September ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "october":
                                dto.October = (dto.October ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "november":
                                dto.November = (dto.November ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "december":
                                dto.December = (dto.December ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "january":
                                dto.January = (dto.January ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "february":
                                dto.February = (dto.February ?? 0) + (decimal)item.TotalTime;
                                break;
                            case "march":
                                dto.March = (dto.March ?? 0) + (decimal)item.TotalTime;
                                break;
                        }
                    }

                    // Calculate totals
                    dto.SumOfTime = (decimal)group.Sum(x => x.TotalTime);
                    dto.SumOfCost = (decimal)group.Sum(x => x.TotalCost);

                    return dto;
                })
                .ToList();

            var dtoList = pivotedData;

            // Apply filtering (search across multiple fields)
            if (!string.IsNullOrEmpty(query.Search))
            {
                dtoList = dtoList.Where(x =>
                    (x.WorkGroup?.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.ParentProject?.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.ProjectTitle?.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            // Get total count before pagination
            int totalCount = dtoList.Count;

            // Apply sorting
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                dtoList = query.SortBy.ToLower() switch
                {
                    "workgroup" => query.Descending
                        ? dtoList.OrderByDescending(x => x.WorkGroup).ToList()
                        : dtoList.OrderBy(x => x.WorkGroup).ToList(),
                    "parentproject" => query.Descending
                        ? dtoList.OrderByDescending(x => x.ParentProject).ToList()
                        : dtoList.OrderBy(x => x.ParentProject).ToList(),
                    "projecttitle" => query.Descending
                        ? dtoList.OrderByDescending(x => x.ProjectTitle).ToList()
                        : dtoList.OrderBy(x => x.ProjectTitle).ToList(),
                    "sumoftime" => query.Descending
                        ? dtoList.OrderByDescending(x => x.SumOfTime).ToList()
                        : dtoList.OrderBy(x => x.SumOfTime).ToList(),
                    "sumofcost" => query.Descending
                        ? dtoList.OrderByDescending(x => x.SumOfCost).ToList()
                        : dtoList.OrderBy(x => x.SumOfCost).ToList(),
                    "percentspent" => query.Descending
                        ? dtoList.OrderByDescending(x => x.PercentSpent).ToList()
                        : dtoList.OrderBy(x => x.PercentSpent).ToList(),
                    _ => dtoList.OrderBy(x => x.WorkGroup).ToList()
                };
            }

            // Apply pagination
            var pagedData = dtoList
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            // Calculate total pages
            int totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            // Extract unique months present in the data (financial year periods 1-12)
            var months = new HashSet<int>();
            foreach (var row in pagedData)
            {
                if (row.April.HasValue) months.Add(1);
                if (row.May.HasValue) months.Add(2);
                if (row.June.HasValue) months.Add(3);
                if (row.July.HasValue) months.Add(4);
                if (row.August.HasValue) months.Add(5);
                if (row.September.HasValue) months.Add(6);
                if (row.October.HasValue) months.Add(7);
                if (row.November.HasValue) months.Add(8);
                if (row.December.HasValue) months.Add(9);
                if (row.January.HasValue) months.Add(10);
                if (row.February.HasValue) months.Add(11);
                if (row.March.HasValue) months.Add(12);
            }

            return new SummarisedWgTimePivotDto
            {
                Months = months.OrderBy(m => m).ToList(),
                Rows = pagedData,
                Pagination = new PaginationDto
                {
                    PageNumber = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalCount
                }
            };
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
