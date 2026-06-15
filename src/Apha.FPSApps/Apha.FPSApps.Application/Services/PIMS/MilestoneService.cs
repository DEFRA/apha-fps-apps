using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class MilestoneService: IMilestoneService
    {
        private readonly IPimsApiClient _client;
        public MilestoneService(IPimsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<MilestoneDto>>> GetAllMilestonesAsync(QueryParameters<string> parameters, string project)
            => await _client.PimsMilestone.GetAllMilestonesAsync(parameters, project);

        public async Task<ApiResponseDto<MilestoneDto>> GetMilestoneAsync(string project, string number)
            => await _client.PimsMilestone.GetMilestoneAsync(project, number);

        public async Task<ApiResponseDto<MilestoneDto>> SaveMilestoneAsync(string project, MilestoneDto dto)
            => await _client.PimsMilestone.SaveMilestoneAsync(project, dto);

        public async Task<ApiResponseDto<MilestoneDto>> UpdateMilestoneAsync(string project, string number, MilestoneDto dto)
            => await _client.PimsMilestone.UpdateMilestoneAsync(project, number, dto);

        public async Task<ApiResponseDto<object>> DeleteMilestoneAsync(string project, string number)
            => await _client.PimsMilestone.DeleteMilestoneAsync(project, number);

        public async Task<ApiResponseDto<object>> UpdateFormRequiredAsync(string parentProject, bool formRequired)
            => await _client.PimsMilestone.UpdateFormRequiredAsync(parentProject, formRequired);

        public async Task<ApiResponseDto<List<MilestoneTypeDto>>> GetMilestoneTypesAsync(string? milestoneDeliverable = null)
            => await _client.PimsMilestone.GetMilestoneTypesAsync(milestoneDeliverable);


        public async Task<ApiResponseDto<List<MilestoneFormDatesDto>>> GetAllMilestoneFormDatesAsync(string parentProject, QueryParameters<string> parameters)
           => await _client.PimsMilestone.GetAllMilestoneFormDatesAsync(parentProject, parameters);

        public async Task<ApiResponseDto<MilestoneFormDatesDto>> GetMilestoneFormDatesAsync(string parentProject, short year)
            => await _client.PimsMilestone.GetMilestoneFormDatesAsync(parentProject, year);

        public async Task<ApiResponseDto<MilestoneFormDatesDto>> SaveMilestoneFormDatesAsync(string parentProject, MilestoneFormDatesDto dto)
            => await _client.PimsMilestone.SaveMilestoneFormDatesAsync(parentProject, dto);

        public async Task<ApiResponseDto<object>> DeleteMilestoneFormDatesAsync(string parentProject, short year)
            => await _client.PimsMilestone.DeleteMilestoneFormDatesAsync(parentProject, year);

        public async Task<ApiResponseDto<List<LogMilestoneDto>>> GetLogMilestonesAsync(QueryParameters<string> parameters,string? project,string? numberPart1,string? numberPart2)
            => await _client.PimsMilestone.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);
    }
}
