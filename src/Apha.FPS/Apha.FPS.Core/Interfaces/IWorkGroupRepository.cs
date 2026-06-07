using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for Workgroup lookup operations.
    /// </summary>
    public interface IWorkGroupRepository
    {
        /// <summary>Returns all WorkGroup names for dropdown population.</summary>
        Task<List<string>> GetAllWorkGroupNamesAsync();

        /// <summary>Returns workgroups filtered by profit centre.</summary>
        Task<List<WorkGroupView>> GetWorkGroupsByProfitCentreAsync(string profitCentre);
    }
}
