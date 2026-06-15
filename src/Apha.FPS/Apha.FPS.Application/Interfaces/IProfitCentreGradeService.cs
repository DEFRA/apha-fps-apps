using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProfitCentreGradeService
    {
        Task<PaginatedResult<ProfitCentreGradeDto>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre);

        Task<PaginatedResult<ProfitCentreGradeDto>> GetAllPagedAsync(QueryParameters<string> query);

        Task<ProfitCentreGradeDto?> GetByIdAsync(string pcGrade);

        Task<ProfitCentreGradeDto> CreateAsync(ProfitCentreGradeDto dto);

        Task<ProfitCentreGradeDto> UpdateAsync(string originalPcGrade, ProfitCentreGradeDto dto);

        Task<bool> DeleteAsync(string pcGrade);

        Task<List<string>> GetAllProfitCentreCodesAsync();

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllPcGradesAsync();
    }
}
