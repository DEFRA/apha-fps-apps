using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Dtos
{
    public class PaginatedApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
        public List<ApiErrorDto>? Errors { get; set; } = new();
        public ApiMetaDto Meta { get; set; } = new();

        public static PaginatedApiResponseDto<T> SuccessResponse(T data, string message = "")
        {
            return new PaginatedApiResponseDto<T>
            {
                Success = true,
                Data = data,
                Meta = new ApiMetaDto
                {
                    //Message = message,
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }

        public static PaginatedApiResponseDto<T> FailureResponse(List<ApiErrorDto>? errors, ApiMetaDto meta)
        {
            return new PaginatedApiResponseDto<T>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto
                {
                    CorrelationId = meta.CorrelationId,
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }

        public static ApiResponseDto<T> ValidationFailure(
        string message,
        Dictionary<string, string[]> validationErrors)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Errors = new List<ApiErrorDto> {
                    new ApiErrorDto
                    {
                        Code = "VALIDATION_ERROR",
                        Message = message,
                        Details = validationErrors
                    }
                },
                Meta = new ApiMetaDto
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }
    }
}
