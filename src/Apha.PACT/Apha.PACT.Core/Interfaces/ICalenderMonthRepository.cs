using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface ICalenderMonthRepository
    {
        Task<IEnumerable<CalenderMonth>> GetCalenderMonthsAsync();
        Task<List<double>> GetValidCalenderMonthsAsync();
    }
}