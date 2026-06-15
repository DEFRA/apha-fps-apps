using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using System.Reflection;
using System.Text.Json;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class BudgetBidsService : IBudgetBidsService
    {
        private readonly IFpsApiClient _fpsClient;

        public BudgetBidsService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup)
        {
            return await _fpsClient.FpsBudgetBids.GetBidViewAsync(workgroup);
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewPagedAsync(QueryParameters<string> query, string workgroup)
        {
            var all = await _fpsClient.FpsBudgetBids.GetBidViewAsync(workgroup);
            if (!all.Success || all.Data == null)
                return all;

            var items = ApplyFilterSortPage(all.Data, query, out var pagination);
            return ApiResponseDto<List<BidViewDto>>.SuccessResponse(items, pagination);
        }

        public async Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string WorkGroupName, string account)
        {
            return await _fpsClient.FpsBudgetBids.GetBidByIdAsync(WorkGroupName, account);
        }

        public async Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetBids.CreateBidAsync(bid);
        }

        public async Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetBids.UpdateBidAsync(bid);
        }

        public async Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetBids.DeleteBidAsync(bid);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            return await _fpsClient.FpsBudgetBids.GetAccountCategoriesAsync();
        }

        private static List<T> ApplyFilterSortPage<T>(List<T> source, QueryParameters<string> query, out PaginationDto pagination)
        {
            var filtered = source.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query.Filter,
                              new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                              ?? new Dictionary<string, string>();
                foreach (var kv in filters)
                {
                    if (string.IsNullOrWhiteSpace(kv.Value)) continue;
                    var prop = typeof(T).GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                        filtered = filtered.Where(item => prop.GetValue(item)?.ToString()?.Contains(kv.Value, StringComparison.OrdinalIgnoreCase) ?? false);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var prop = typeof(T).GetProperty(query.SortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                    filtered = query.Descending
                        ? filtered.OrderByDescending(i => prop.GetValue(i))
                        : filtered.OrderBy(i => prop.GetValue(i));
            }

            var allItems = filtered.ToList();
            var totalRecords = allItems.Count;
            var pageSize   = query.PageSize > 0 ? query.PageSize : 10;
            var pageNumber = query.Page    > 0 ? query.Page    : 1;
            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0;

            pagination = new PaginationDto
            {
                PageNumber   = pageNumber,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                TotalPages   = totalPages
            };

            return allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
