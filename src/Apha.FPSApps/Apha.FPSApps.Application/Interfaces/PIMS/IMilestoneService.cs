using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IMilestoneService
    {
        Task<ApiResponseDto<List<MilestoneDto>>> GetAllMilestonesAsync(QueryParameters<string> parameters, string project);
        Task<ApiResponseDto<MilestoneDto>> GetMilestoneAsync(string project, string number);
        Task<ApiResponseDto<MilestoneDto>> SaveMilestoneAsync(string project, MilestoneDto dto);
        Task<ApiResponseDto<MilestoneDto>> UpdateMilestoneAsync(string project, string number, MilestoneDto dto);
        Task<ApiResponseDto<object>> DeleteMilestoneAsync(string project, string number);
        Task<ApiResponseDto<object>> UpdateFormRequiredAsync(string parentProject, bool formRequired);

        Task<ApiResponseDto<List<MilestoneTypeDto>>> GetMilestoneTypesAsync(string? milestoneDeliverable = null);

        Task<ApiResponseDto<List<MilestoneFormDatesDto>>> GetAllMilestoneFormDatesAsync(string parentProject, QueryParameters<string> parameters);
        Task<ApiResponseDto<MilestoneFormDatesDto>> GetMilestoneFormDatesAsync(string parentProject, short year);
        Task<ApiResponseDto<MilestoneFormDatesDto>> SaveMilestoneFormDatesAsync(string parentProject, MilestoneFormDatesDto dto);
        Task<ApiResponseDto<object>> DeleteMilestoneFormDatesAsync(string parentProject, short year);

        Task<ApiResponseDto<List<LogMilestoneDto>>> GetLogMilestonesAsync(QueryParameters<string> parameters, string? project, string? numberPart1, string? numberPart2);
    }
}
