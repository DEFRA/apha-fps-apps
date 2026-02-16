using Apha.Common.Contracts;
using System.Net.Http.Json;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public class HttpExecutor : IHttpExecutor
    {
        private readonly HttpClient _http;
        public HttpExecutor(HttpClient http)
        {
            _http = http;
        }
        public async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            var response = await _http.GetAsync(url);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> PostAsync<TRequest, T>(
            string url,
            TRequest body)
        {
            var response = await _http.PostAsJsonAsync(url, body);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> PutAsync<TRequest, T>(
            string url,
            TRequest body)
        {
            var response = await _http.PutAsJsonAsync(url, body);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string url)
        {
            var response = await _http.DeleteAsync(url);
            return await response.ToApiResponse<T>();
        }
    }
}
