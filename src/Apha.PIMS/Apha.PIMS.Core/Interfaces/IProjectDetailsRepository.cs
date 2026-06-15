using Apha.PIMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProjectDetailsRepository
    {
        Task<ProjectDetail?> GetPimsDetailAsync(string parentproject);
        Task<ProjectDetail> AddPimsDetailAsync(ProjectDetail entity);
        Task<ProjectDetail> UpdatePimsDetailAsync(ProjectDetail entity);
        Task<ProposedProject?> GetProposedProjectAsync(string parentproject);
        Task<ProposedProject> UpdateProposedProjectAsync(ProposedProject entity, string transferTo);
        Task<List<Risk>> GetAllRiskAsync();
        Task<List<Year>> GetAllYearAsync();
        Task<Project?> GetFpsProjectByIdAsync(string parentproject);
    }
}
