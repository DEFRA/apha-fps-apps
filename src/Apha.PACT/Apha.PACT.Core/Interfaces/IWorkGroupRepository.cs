using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IWorkGroupRepository
    {
        Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync();
        Task<PactProfitCentreView?> GetProfitCentreAsync(string profitCentre);
        Task<IEnumerable<WorkGroup>> GetWorkGroupsForEmailAsync(string profitCentre);
        Task<IEnumerable<TimeSheetTemplateRow>> GetTimeSheetTemplateAsync(string workGroup, short month, short layout);
        Task<IEnumerable<OutputSheetTemplateRow>> GetOutputSheetTemplateAsync(string workGroup, short month);
        Task<PagedData<WorkGroupTimeCode>> GetWorkGroupTimeCodeAsync(PaginationParameters<string> query, string? workGroup, int? monthNumber);
        Task<PagedData<WorkGroupValidTimeCode>> GetWorkGroupValidTimeCodeAsync(PaginationParameters<string> query, string workGroup);
        Task<IEnumerable<WgSummarisedStaffTimeUsageView>> GetWgSummarisedStaffTimeUsageAsync(string workGroup);
        Task<PagedData<WorkGroup>> GetWorkGroupsByProfitCentreAsync(PaginationParameters<string> query, string profitCentre);
        Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag);
        Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient);
    }
}
