using Apha.Common.Contracts;
using System.Net.Http.Json;
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
                var apiResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);

                if (apiResponse != null)
                    return apiResponse;
            }
            catch(Exception ex) 
            {
                throw ex;
            }            

            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError>{ new ApiError { Code = response.StatusCode.ToString(), Message = response.ReasonPhrase! } },
                Meta = new ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Extension method for paginated API responses
        /// </summary>
        public static async Task<PaginatedApiResponse<T>> ToPaginatedApiResponse<T>(
            this HttpResponseMessage response)
        {
            try
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<T>>(JsonOptions);

                if (apiResponse != null)
                    return apiResponse;
            }
            catch (Exception ex)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Failed to deserialize paginated API response. Status: {response.StatusCode}, Content: {content}",
                    ex);
            }

            return new PaginatedApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError>
                {
                    new ApiError
                    {
                        Code = response.StatusCode.ToString(),
                        Message = response.ReasonPhrase ?? "Unknown error"
                    }
                },
                Meta = new ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }
    }
}
