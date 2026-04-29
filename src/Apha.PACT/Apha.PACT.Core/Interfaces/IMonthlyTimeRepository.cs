namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyTimeRepository
    {
        Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject);
    }
}
