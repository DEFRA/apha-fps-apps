using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsReviewItemApiClient
    {
        Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync();

        // TRANSFORMENGINE: GET /api/v1/reviewitem/paged — paged/sorted/filterable list
        Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GET /api/v1/reviewitem/{itemid:int}
        Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId);

        // TRANSFORMENGINE: POST /api/v1/reviewitem
        Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto);

        // TRANSFORMENGINE: PUT /api/v1/reviewitem/{itemid:int} — route PK is authoritative
        Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/reviewitem/{itemid:int}
        Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId);
    }
}
