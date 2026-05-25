using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IDivisionGradeService
    {
        Task<PaginatedResult<DivisionGradeDto>> GetAllPagedAsync(QueryParameters<string> query);
        Task<DivisionGradeDto?> GetByIdAsync(string divisionGradeCode);
        Task<DivisionGradeDto> CreateAsync(DivisionGradeDto dto);
        Task<DivisionGradeDto> UpdateAsync(string originalCode, DivisionGradeDto dto);
        Task<bool> DeleteAsync(string divisionGradeCode);
        Task<List<string>> GetAllGradeCodesAsync();
    }
}
