using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface ICalenderMonthService
    {
        Task<IEnumerable<CalenderMonthDto>> GetAllCalenderMonthsAsync();
    }
}
