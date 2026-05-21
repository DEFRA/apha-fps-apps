using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProfitCentreService
    {
        Task<IEnumerable<ProfitCentreSettingsDto>> GetAllProfitCentresAsync();
        Task<ProfitCentreSettingsDto?> GetProfitCentreSettingsAsync(string profitCentre);
        Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout);
    }
}
