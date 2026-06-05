using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactMonthHourApiClient
    {
        Task<ApiResponseDto<List<MonthHourDto>>> GetAllAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<MonthHourDto>>> GetByYearAsync(short year);
        Task<ApiResponseDto<List<short>>> GetDistinctYearsAsync();
    }
}
