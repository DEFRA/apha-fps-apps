using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProfitCentreService
    {
        Task<List<ProfitCentreDto>> GetProfitCentresAsync(CancellationToken cancellationToken = default);
    }
}
