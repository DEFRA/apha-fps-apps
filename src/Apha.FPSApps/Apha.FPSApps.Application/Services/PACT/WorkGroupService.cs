using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Validation;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IPactApiClient _pactApiClient;

        public WorkGroupService(IPactApiClient pactApiClient)
        {
            _pactApiClient = pactApiClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
            => await _pactApiClient.PactWorkGroup.GetAllWorkGroupsAsync();

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            return await _pactApiClient.PactWorkGroup.GetPagedWorkGroupTimeCodesAsync(query, workGroup, monthNumber);
        }

        public async Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            return await _pactApiClient.PactWorkGroup.GetPagedWorkGroupValidTimeCodesAsync(query, workGroup);
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            return await _pactApiClient.PactWorkGroup.GetWorkGroupsByProfitCentreAsync(query, profitCentre);
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            return await _pactApiClient.PactWorkGroup.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            return await _pactApiClient.PactWorkGroup.SetSendEmailForAllWorkGroupsAsync(flag);
        }

        public async Task<ApiResponseDto<bool>> UpdateWorkGroupEmailAsync(
            string workGroupName, short sendEmail, string? emailRecipient)
        {
            return await _pactApiClient.PactWorkGroup.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        public async Task<ApiResponseDto<WgSummarisedStaffTimeUsageDto>> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string staffName)
        {
            ValidateStaffName(staffName);
            return await _pactApiClient.PactWorkGroup.GetWgSummarisedStaffTimeUsageAsync(query, staffName);
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsFlaggedForCos90Async()
            => await _pactApiClient.PactWorkGroup.GetWorkGroupsFlaggedForCos90Async();

        public async Task<ApiResponseDto<bool>> SetCos90ForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
            => await _pactApiClient.PactWorkGroup.SetCos90ForProfitCentreWorkGroupsAsync(profitCentre, flag);

        public async Task<ApiResponseDto<bool>> SetCos90ForAllWorkGroupsAsync(short flag)
            => await _pactApiClient.PactWorkGroup.SetCos90ForAllWorkGroupsAsync(flag);

        public async Task<ApiResponseDto<bool>> SetCos90ForWorkGroupAsync(string profitCentre, string workGroupName, short flag)
            => await _pactApiClient.PactWorkGroup.SetCos90ForWorkGroupAsync(profitCentre, workGroupName, flag);

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }

        private static void ValidateStaffName(string satffName)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(satffName))
                errors.Add(new BusinessValidationError("Staff Name is required", "STAFFNAME_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }
    }
}