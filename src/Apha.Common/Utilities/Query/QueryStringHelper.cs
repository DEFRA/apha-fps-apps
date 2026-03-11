using Microsoft.AspNetCore.WebUtilities;

namespace Apha.Common.Utilities.Query
{
    public static class QueryStringHelper
    {
        public static string AddQueryString<T>(string url, T request)
        {
            var dict = request?.GetType()
                              .GetProperties()
                              .Where(p => p.GetValue(request) != null)
                              .ToDictionary(
                                  p => p.Name,
                                  p => p.GetValue(request)!.ToString()
                              );

            return QueryHelpers.AddQueryString(url, dict);
        }
    }
}
