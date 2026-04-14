using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class WorkGroupTestCapabilityService : IWorkGroupTestCapabilityService
    {
        private readonly IPactApiClient _pactClient;

        public WorkGroupTestCapabilityService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup)
            => await _pactClient.PactWorkGroupTestCapability.GetPagedByWorkGroupAsync(query, workGroup);

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode)
            => await _pactClient.PactWorkGroupTestCapability.GetPagedByTestCodeAsync(query, testCode);

        public async Task<ApiResponseDto<TestCapabilityDto>> GetTestCapabilityByIdAsync(string testCode, string workGroup)
            => await _pactClient.PactWorkGroupTestCapability.GetTestCapabilityByIdAsync(testCode, workGroup);

        public async Task<ApiResponseDto<TestCapabilityDto>> CreateTestCapabilityAsync(TestCapabilityDto dto)
            => await _pactClient.PactWorkGroupTestCapability.CreateTestCapabilityAsync(dto);

        public async Task<ApiResponseDto<TestCapabilityDto>> UpdateTestCapabilityAsync(TestCapabilityDto dto)
            => await _pactClient.PactWorkGroupTestCapability.UpdateTestCapabilityAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteTestCapabilityAsync(string testCode, string workGroup)
            => await _pactClient.PactWorkGroupTestCapability.DeleteTestCapabilityAsync(testCode, workGroup);

        //public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync()
        //    => await _pactClient.PactWorkGroupTestCapability.GetAllTestorProductsAsync();

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
            => await _pactClient.PactWorkGroup.GetAllWorkGroupsAsync();
    }
}
