using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IWorkGroupService
    {
        Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync();
        Task<List<string>> GetAllWorkGroupNamesAsync();
        Task<List<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre);
        Task<PaginatedResult<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(QueryParameters<string> query, string profitCentre);
        Task<PaginatedResult<WorkGroupTimeCodeDto>> GetWorkGroupTimeCodeAsync(QueryParameters<string> query, string workGroup, int monthNumber);
        Task<PaginatedResult<WorkGroupValidTimeCodeDto>> GetWorkGroupValidTimeCodeAsync(QueryParameters<string> query, string workGroup);
        Task<WgSummarisedStaffTimeUsageDto> GetWgSummarisedStaffTimeUsageAsync(QueryParameters<string> query, string staffName);
        Task<SummarisedWgTimeViewDto> GetSummarisedWorkgroupTimeSummaryAsync(QueryParameters<string> query, string workGroup);
        Task<PaginatedResult<WorkGroupDto>> GetWorkGroupsByProfitCentreAsync(QueryParameters<string> query, string profitCentre);
        Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag);
        Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient);

        // COS90
        Task<IEnumerable<Cos90WorkGroupDto>> GetWorkGroupsFlaggedForCos90Async();
        Task<bool> SetCos90ForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<bool> SetCos90ForAllWorkGroupsAsync(short flag);
        Task<bool> SetCos90ForWorkGroupAsync(string profitCentre, string workGroupName, short flag);
    }
}
