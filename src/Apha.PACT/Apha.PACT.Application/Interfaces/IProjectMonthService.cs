using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProjectMonthService
    {
        Task<IList<ProjectMonthDto>> GetProjectMonthByProjectAsync(string project);
        Task<ProjectMonthDto?> GetProjectMonthAsync(string project, int monthNo);
        Task<ProjectMonthDto> CreateProjectMonthAsync(ProjectMonthDto dto);
        Task<ProjectMonthDto> UpdateProjectMonthAsync(ProjectMonthDto dto);
        Task<bool> DeleteProjectMonthAsync(string project, int monthNo);
    }
}