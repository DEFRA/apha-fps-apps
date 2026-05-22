using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactCalenderMonthApiClient
    {
        Task<ApiResponseDto<List<CalenderMonthDto>>> GetCalenderMonthsAsync();
    }
}