using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProjectListService
    {
        Task<PaginatedResult<ProjectListViewDto>> GetAllProjectsAsync(QueryParameters<string> query, int showWhichProjects = 2);
        Task<List<ProjectListViewDto>> GetAllProjectsForDropDownAsync();
        Task<List<ProjectsDto>> GetYearlyDetailsByProjectAsync(string parentproject);
    }
}
