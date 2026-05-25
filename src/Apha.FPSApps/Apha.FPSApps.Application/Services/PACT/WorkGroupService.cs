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
        private readonly IPactApiClient _pactClient;

        public WorkGroupService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
            => await _pactClient.PactWorkGroup.GetAllWorkGroupsAsync();

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            return await _pactClient.PactWorkGroup.GetPagedWorkGroupTimeCodesAsync(query, workGroup, monthNumber);
        }

        public async Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            return await _pactClient.PactWorkGroup.GetPagedWorkGroupValidTimeCodesAsync(query, workGroup);
        }

        public async Task<ApiResponseDto<WorkGroupTimeByJobCodeDto>> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            return await _pactClient.PactWorkGroup.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);
        }

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }
    }
}