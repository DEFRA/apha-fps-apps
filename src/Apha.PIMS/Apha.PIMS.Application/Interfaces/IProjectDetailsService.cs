using Apha.PIMS.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProjectDetailsService
    {
        Task<ProjectDetailDto?> GetPimsDetailAsync(string parentproject);
        Task<ProjectDetailDto> SavePimsDetailAsync(ProjectDetailDto dto);
        Task<ProposedProjectDto?> GetProposedProjectAsync(string parentproject);
        Task<ProposedProjectDto> UpdateProposedProjectAsync(ProposedProjectDto dto);
        Task<List<RiskDto>> GetAllRiskAsync();
    }
}
