namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyTimeRepository
    {
        Task<bool> HasDependentRowsAsync(string workGroup, string timeCode, string parentProject);
    }
}
