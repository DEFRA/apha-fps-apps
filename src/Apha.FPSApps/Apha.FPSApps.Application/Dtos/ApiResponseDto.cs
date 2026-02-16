using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Validation;

namespace Apha.FPSApps.Application.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public List<ApiErrorDto>? Errors { get; set; } = new();
        public ApiMetaDto Meta { get; set; } = new();

        public static ApiResponseDto<T> SuccessResponse(T data, string message = "")
        {
            return new ApiResponseDto<T>
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

        public static ApiResponseDto<T> FailureResponse(List<ApiErrorDto>? errors, ApiMetaDto meta)
        {
            return new ApiResponseDto<T>
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

    public class ApiErrorDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public class ApiMetaDto
    {
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
