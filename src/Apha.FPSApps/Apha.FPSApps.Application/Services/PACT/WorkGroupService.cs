using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Validation;
using System.Reflection;
using System.Text.Json;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IPactApiClient _pactApiClient;

        public WorkGroupService(IPactApiClient pactApiClient)
        {
            _pactApiClient = pactApiClient;
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync()
            => await _pactApiClient.PactWorkGroup.GetAllWorkGroupNamesAsync();

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre)
            => await _pactApiClient.PactWorkGroup.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var all = await _pactApiClient.PactWorkGroup.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
            if (!all.Success || all.Data == null)
                return all;

            var items = ApplyFilterSortPage(all.Data, query, out var pagination);
            return ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(items, pagination);
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
            => await _pactApiClient.PactWorkGroup.GetAllWorkGroupsAsync();

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            return await _pactApiClient.PactWorkGroup.GetPagedWorkGroupTimeCodesAsync(query, workGroup, monthNumber);
        }

        public async Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            return await _pactApiClient.PactWorkGroup.GetPagedWorkGroupValidTimeCodesAsync(query, workGroup);
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            return await _pactApiClient.PactWorkGroup.GetWorkGroupsByProfitCentreAsync(query, profitCentre);
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            return await _pactApiClient.PactWorkGroup.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            return await _pactApiClient.PactWorkGroup.SetSendEmailForAllWorkGroupsAsync(flag);
        }

        public async Task<ApiResponseDto<bool>> UpdateWorkGroupEmailAsync(
            string workGroupName, short sendEmail, string? emailRecipient)
        {
            return await _pactApiClient.PactWorkGroup.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        public async Task<ApiResponseDto<WgSummarisedStaffTimeUsageDto>> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string staffName)
        {
            ValidateStaffName(staffName);
            return await _pactApiClient.PactWorkGroup.GetWgSummarisedStaffTimeUsageAsync(query, staffName);
        }

        private static List<T> ApplyFilterSortPage<T>(List<T> source, QueryParameters<string> query, out PaginationDto pagination)
        {
            var filtered = ApplyFilter<T>(source, query.Filter);
            filtered = ApplySort<T>(filtered, query.SortBy, query.Descending);

            var allItems     = filtered.ToList();
            var totalRecords = allItems.Count;
            var pageSize     = query.PageSize > 0 ? query.PageSize : 5;
            var pageNumber   = query.Page     > 0 ? query.Page     : 1;
            var totalPages   = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0;

            pagination = new PaginationDto
            {
                PageNumber   = pageNumber,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                TotalPages   = totalPages
            };

            return allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        private static IEnumerable<T> ApplyFilter<T>(IEnumerable<T> source, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return source;

            var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filter,
                          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                          ?? new Dictionary<string, string>();

            foreach (var kv in filters)
            {
                if (string.IsNullOrWhiteSpace(kv.Value)) continue;
                var prop = typeof(T).GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                    source = source.Where(item => prop.GetValue(item)?.ToString()?.Contains(kv.Value, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            return source;
        }

        private static IEnumerable<T> ApplySort<T>(IEnumerable<T> source, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return source;

            var prop = typeof(T).GetProperty(sortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
                return descending
                    ? source.OrderByDescending(i => prop.GetValue(i))
                    : source.OrderBy(i => prop.GetValue(i));

            return source;
        }

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }

        private static void ValidateStaffName(string satffName)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(satffName))
                errors.Add(new BusinessValidationError("Staff Name is required", "STAFFNAME_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }
    }
}