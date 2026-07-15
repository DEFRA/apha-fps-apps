using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Stage 2 Check Resource Allocation
    /// (frmResourceMain2) read-only grid data.
    /// </summary>
    public interface IResourceMain2Service
    {
        /// <summary>Returns staff allocation rows for the given workgroup grade.</summary>
        Task<List<ResourceStaffAllocationDto>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade);

        /// <summary>Returns job rows for the given staff member.</summary>
        Task<List<ResourceStaffJobDto>> GetStaffJobsByStaffIdAsync(int staffId);
    }
}
