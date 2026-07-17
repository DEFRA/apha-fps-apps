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
        Task<PaginatedResult<MilestoneDto>> GetPMDMilestonesAsync(QueryParameters<string> parameters, string project);
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

        // Staging / Import operations
        Task<List<StagingMilestoneDto>> GetStagingRowsAsync(string? project);

        Task<PaginatedResult<StagingMilestoneDto>> GetAllStagingRowsAsync(QueryParameters<string> parameters);
        Task<StagingMilestoneDto> AddStagingRowAsync(StagingMilestoneDto dto, int year);
        Task<StagingMilestoneDto> UpdateStagingRowAsync(StagingMilestoneDto dto);
        Task<bool> DeleteStagingRowAsync(int id);
        Task<int> ClearStagingAsync(string project);
        Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode);
        Task<int> ImportStagingAsync(string project, string? changedBy = null);
        Task<int> ImportWithOverwriteAsync(string project, string? changedBy = null);
        Task<string> GetNextMilestoneNumberAsync(string project, int year);

        Task<List<ProjectYearManagerDto>> GetProjectYearManagersAsync(int year);
    }
}
