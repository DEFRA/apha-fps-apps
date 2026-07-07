using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITotalBusinessOverheadsRepository
    {
        Task<TotalBusinessOverheads?> GetByYearAsync(int fpsYear);
        Task<TotalBusinessOverheads> UpdateAsync(TotalBusinessOverheads entity);
    }
}
