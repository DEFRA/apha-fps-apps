using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IDivisionGradeRepository
    {
        Task<PagedData<DivisionGrade>> GetAllPagedAsync(PaginationParameters<string> query);
        Task<DivisionGrade?> GetByIdAsync(string divisionGradeCode);
        Task<DivisionGrade> CreateAsync(DivisionGrade divisionGrade);
        Task<DivisionGrade> UpdateAsync(string originalCode, DivisionGrade divisionGrade);
        Task<bool> DeleteAsync(string divisionGradeCode);
        Task<List<string>> GetAllGradeCodesAsync();
    }
}
