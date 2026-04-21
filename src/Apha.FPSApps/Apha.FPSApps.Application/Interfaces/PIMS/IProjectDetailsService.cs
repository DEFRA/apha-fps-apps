using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IProjectDetailsService
    {
        Task<ApiResponseDto<ProjectDetailDto>> GetPimsDetailAsync(string parentproject);
        Task<ApiResponseDto<ProjectDetailDto>> SavePimsDetailAsync(string parentproject, ProjectDetailDto dto);
        Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectAsync(string parentproject);
        Task<ApiResponseDto<ProposedProjectDto>> UpdateProposedProjectAsync(string parentproject, ProposedProjectDto dto);
        Task<ApiResponseDto<List<RiskDto>>> GetAllRiskAsync();
        Task<ApiResponseDto<List<YearDto>>> GetAllYearAsync();
    }
}
