using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProfitCentreRepository
    {
        Task<IEnumerable<PactProfitCentreView>> GetAllProfitCentresAsync();
        Task<PactProfitCentreView?> GetProfitCentreSettingsAsync(string profitCentre);
        Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout);
    }
}
