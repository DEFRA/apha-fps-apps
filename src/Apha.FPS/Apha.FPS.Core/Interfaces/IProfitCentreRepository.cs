using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProfitCentreRepository
    {
        Task<List<ProfitCentreView>> GetProfitCentresAsync();
        Task<IEnumerable<ProfitCentre>> GetAllProfitCentresAsync();
        Task<ProfitCentre?> GetProfitCentreByIdAsync(string profitCentre);
        Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout);
    }
}
