using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for the Stage 2 Check Resource Allocation (frmResourceMain2)
    /// read-only grids.
    /// </summary>
    public interface IResourceMain2Repository
    {
        /// <summary>Returns the staff-of-grade allocation rows for a workgroup grade.</summary>
        Task<List<ResourceStaffAllocationView>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade);

        /// <summary>Returns the jobs-for-staff rows for a given staff member.</summary>
        Task<List<ResourceStaffJobView>> GetStaffJobsByStaffIdAsync(int staffId);
    }
}
