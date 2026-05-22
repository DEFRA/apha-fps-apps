using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProfitCentreService
    {
        Task<List<ProfitCentreDto>> GetProfitCentresAsync();
        Task<IEnumerable<ProfitCentreDto>> GetAllProfitCentresAsync();
        Task<ProfitCentreDto?> GetProfitCentreByIdAsync(string profitCentre);
        Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout);
    }
}
