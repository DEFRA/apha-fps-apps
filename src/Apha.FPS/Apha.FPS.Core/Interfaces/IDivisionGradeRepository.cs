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
        Task<List<string>> GetAllDivisionGradeCodesAsync();
        /// <summary>Returns true if any DivisionGrade row references the given GradeCode.</summary>
        Task<bool> ExistsForGradeCodeAsync(string gradeCode);
    }
}
