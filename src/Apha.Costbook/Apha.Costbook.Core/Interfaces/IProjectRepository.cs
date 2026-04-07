using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<PagedData<Project>> GetPaginatedProjectsAsync(PaginationParameters<string> queryFilter);

        Task<IEnumerable<Project>> GetProjectsAsync(string? contractFilter, string? submittedByFilter);
        Task<Project?> GetProjectByIdAsync(string id);
        Task<Project> AddProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(string id);
        Task<string> GetNextProjectNumberAsync(string? baseNumber);
    }
}
