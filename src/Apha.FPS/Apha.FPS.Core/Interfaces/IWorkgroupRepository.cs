namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for Workgroup lookup operations.
    /// </summary>
    public interface IWorkgroupRepository
    {
        /// <summary>Returns all Workgroup names for dropdown population.</summary>
        Task<List<string>> GetAllWorkgroupNamesAsync();
    }
}
