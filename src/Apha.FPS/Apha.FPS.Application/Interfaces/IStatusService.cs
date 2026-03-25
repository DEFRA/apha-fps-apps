namespace Apha.FPS.Application.Interfaces
{
    public interface IStatusService
    {
        Task<IEnumerable<string>> GetAllStatusesAsync();
    }
}
