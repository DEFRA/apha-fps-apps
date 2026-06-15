using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class ProjectDetailsService : IProjectDetailsService
    {
        private readonly IPimsApiClient _client;

        public ProjectDetailsService(IPimsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<ProjectDetailDto>> GetPimsDetailAsync(string parentproject)
            => await _client.PimsProjectDetails.GetPimsDetailAsync(parentproject);

        public async Task<ApiResponseDto<ProjectDetailDto>> SavePimsDetailAsync(string parentproject, ProjectDetailDto dto)
            => await _client.PimsProjectDetails.SavePimsDetailAsync(parentproject, dto);

        public async Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectAsync(string parentproject)
            => await _client.PimsProjectDetails.GetProposedProjectAsync(parentproject);

        public async Task<ApiResponseDto<ProposedProjectDto>> UpdateProposedProjectAsync(string parentproject, ProposedProjectDto dto)
            => await _client.PimsProjectDetails.UpdateProposedProjectAsync(parentproject, dto);

        public async Task<ApiResponseDto<List<RiskDto>>> GetAllRiskAsync()
            => await _client.PimsProjectDetails.GetAllRiskAsync();

        public async Task<ApiResponseDto<List<YearDto>>> GetAllYearAsync()
            => await _client.PimsProjectDetails.GetAllYearAsync();

        public async Task<ApiResponseDto<ProjectDto>> GetFpsProjectAsync(string parentproject)
            => await _client.PimsProjectDetails.GetFpsProjectAsync(parentproject);
    }
}
