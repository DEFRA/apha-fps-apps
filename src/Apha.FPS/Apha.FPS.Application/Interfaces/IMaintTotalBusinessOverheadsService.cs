using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMaintTotalBusinessOverheadsService
    {
        Task<TotalBusinessOverheadsDto?> GetAsync();
        Task<TotalBusinessOverheadsDto> UpdateAsync(TotalBusinessOverheadsDto dto);
    }
}
