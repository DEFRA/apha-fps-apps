using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IMilestoneRepository
    {
        Task<PagedData<Milestone>> GetAllMilestonesAsync(PaginationParameters<string> parameters, string project);
        Task<PagedData<Milestone>> GetPMDMilestonesAsync(PaginationParameters<string> parameters, string project);
        Task<Milestone?> GetMilestoneAsync(string project, string number);
        Task<Milestone> AddMilestoneAsync(Milestone entity, string? changedBy);
        Task<Milestone> UpdateMilestoneAsync(Milestone entity, string? changedBy);
        Task<bool> DeleteMilestoneAsync(string project, string number);
        Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired);
        // Lookup
        Task<List<MilestoneType>> GetMilestoneTypesAsync(string? milestoneDeliverable = null);

        // MilestoneFormDates operations
        Task<PagedData<MilestoneFormDates>> GetAllMilestoneFormDatesAsync(PaginationParameters<string> parameters, string parentProject);
        Task<MilestoneFormDates?> GetMilestoneFormDatesAsync(short year, string parentProject);
        Task<MilestoneFormDates> AddMilestoneFormDatesAsync(MilestoneFormDates entity);
        Task<MilestoneFormDates> UpdateMilestoneFormDatesAsync(MilestoneFormDates entity);
        Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject);

        // Log Milestone operations
        Task<PagedData<LogMilestone>> GetLogMilestonesAsync(PaginationParameters<string> parameters, string? project, string? numberPart1, string? numberPart2);
        // Staging / Import operations
        Task<List<StagingMilestone>> GetStagingRowsAsync(string? project);

        Task<PagedData<StagingMilestone>> GetAllStagingRowsAsync(PaginationParameters<string> parameters);
        Task<StagingMilestone> AddStagingRowAsync(StagingMilestone entity);
        Task<StagingMilestone> UpdateStagingRowAsync(StagingMilestone entity);
        Task<bool> DeleteStagingRowAsync(int id);
        Task<int> ClearStagingAsync(string project);
        Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode);
        Task<int> ImportStagingAsync(string project, string? changedBy);
        Task<int> ImportWithOverwriteAsync(string project, string? changedBy);
        Task<string> GetNextMilestoneNumberAsync(string project, int year);

        // Project Year Manager operations
        Task<List<ProjectYearManager>> GetProjectYearManagersAsync(int year);
    }
}
