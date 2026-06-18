using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces;

public interface IProjectYearRepository
{
    Task<IEnumerable<ProjectYear>> GetByProjectAsync(string project);
    Task<int?> GetMaxProjectYearAsync(string project);
    Task<ProjectYear> AddProjectYearAsync(string project, int year, ProjectYear yearData);
    Task<ProjectYear> UpdateProjectYearAsync(ProjectYear projectYear);
    Task<(bool Deleted, IReadOnlyList<string> Errors)> DeleteProjectYearAsync(string project, int year);
   
}
