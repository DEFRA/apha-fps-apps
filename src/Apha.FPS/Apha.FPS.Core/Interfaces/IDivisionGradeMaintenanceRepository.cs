using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IDivisionGradeMaintenanceRepository
    {
        Task<PagedData<DivisionGradeMaintenance>> GetAllPagedAsync(PaginationParameters<string> query);
        Task<DivisionGradeMaintenance?> GetByIdAsync(string divisionGradeCode);
        Task<DivisionGradeMaintenance> CreateAsync(DivisionGradeMaintenance divisionGrade);
        Task<DivisionGradeMaintenance> UpdateAsync(string originalCode, DivisionGradeMaintenance divisionGrade);
        Task<bool> DeleteAsync(string divisionGradeCode);
        Task<List<string>> GetAllGradeCodesAsync();
    }
}
