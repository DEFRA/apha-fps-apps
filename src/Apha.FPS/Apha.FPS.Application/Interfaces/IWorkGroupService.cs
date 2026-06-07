using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Workgroup lookup operations.
    /// </summary>
    public interface IWorkGroupService
    {
        /// <summary>Returns all WorkGroup names for dropdown population.</summary>
        Task<List<string>> GetAllWorkGroupNamesAsync();

        /// <summary>Returns workgroups filtered by profit centre.</summary>
        Task<List<WorkGroupViewDto>> GetWorkGroupsAsync(string profitCentre);
    }
}
