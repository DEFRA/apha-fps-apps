using Apha.Common.Contracts;
using System.Text.Json;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public static class HttpResponseExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static async Task<ApiResponse<T>> ToApiResponse<T>(
         this HttpResponseMessage response)
        {
            try
            {
                string content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);

                    if (apiResponse != null)
                        return apiResponse;
                }
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
