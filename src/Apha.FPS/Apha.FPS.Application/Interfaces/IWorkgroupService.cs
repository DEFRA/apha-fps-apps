namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Workgroup lookup operations.
    /// </summary>
    public interface IWorkgroupService
    {
        /// <summary>Returns all Workgroup names for dropdown population.</summary>
        Task<List<string>> GetAllWorkgroupNamesAsync(CancellationToken cancellationToken = default);
    }
}
