using Apha.Common.Contracts;
using System.Text.Json;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public static class HttpResponseExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task<ApiResponse<T>> ToApiResponse<T>(
         this HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
                    if (apiResponse != null)
                        return apiResponse;
                }

                if (response.IsSuccessStatusCode)
                    return new ApiResponse<T> { Success = true };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while processing the response.", ex);
            }

            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Code = response.StatusCode.ToString(), Message = response.ReasonPhrase! } },
                Meta = new ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }
    }
}
