using System.Net.Http.Json;
using Apha.Common.Contracts;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public static class HttpResponseExtensions
    {
        public static async Task<ApiResponse<T>> ToApiResponse<T>(
         this HttpResponseMessage response)
        {
            try
            {
                var apiResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponse<T>>();

                if (apiResponse != null)
                    return apiResponse;
            }
            catch(Exception e) 
            {
                throw e;
            }

            //return new ApiResponse<T>.FailureResponse(
            //    $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
            //);

            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError>{ new ApiError { Code = response.StatusCode.ToString(), Message = response.ReasonPhrase } },
                Meta = new ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }
    }
}
