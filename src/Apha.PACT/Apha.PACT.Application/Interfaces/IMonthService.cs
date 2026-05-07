using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthService
    {
        Task<IEnumerable<MonthDto>> GetAllMonthsAsync();
    }
}
