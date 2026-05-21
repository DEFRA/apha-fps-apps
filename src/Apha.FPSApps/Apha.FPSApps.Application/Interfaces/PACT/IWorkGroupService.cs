using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IWorkGroupService
    {
        Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync();
        Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(QueryParameters<string> query, string profitCentre);
        Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag);
        Task<ApiResponseDto<bool>> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient);
    }
}
