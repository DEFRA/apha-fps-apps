using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IMilestoneService
    {
        Task<PaginatedResult<MilestoneDto>> GetAllMilestonesAsync(QueryParameters<string> parameters, string project);
        Task<MilestoneDto?> GetMilestoneAsync(string project, string number);
        Task<MilestoneDto> SaveMilestoneAsync(MilestoneDto dto, string? changedBy = null);
        Task<MilestoneDto> UpdateMilestoneAsync(MilestoneDto dto, string? changedBy = null);
        Task<bool> DeleteMilestoneAsync(string project, string number);
        Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired);

        Task<List<MilestoneTypeDto>> GetMilestoneTypesAsync(string? milestoneDeliverable = null);


        Task<PaginatedResult<MilestoneFormDatesDto>> GetAllMilestoneFormDatesAsync(QueryParameters<string> parameters, string parentProject);
        Task<MilestoneFormDatesDto?> GetMilestoneFormDatesAsync(short year, string parentProject);
        Task<MilestoneFormDatesDto> SaveMilestoneFormDatesAsync(MilestoneFormDatesDto dto);
        Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject);

        Task<PaginatedResult<LogMilestoneDto>> GetLogMilestonesAsync(QueryParameters<string> parameters, string? project, string? numberPart1, string? numberPart2);
    }
}
