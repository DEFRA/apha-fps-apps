using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsResourceMain2ApiClient : IFpsResourceMain2ApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsResourceMain2ApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ResourceStaffAllocationDto>>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade)
        {
            var url = string.Format(FpsApiEndpoints.GetResourceStaffAllocations, Uri.EscapeDataString(workGroupGrade));
            var response = await _http.GetAsync<List<ResourceStaffAllocationRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceStaffAllocationDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceStaffAllocationDto>>>(response);
            return ApiResponseDto<List<ResourceStaffAllocationDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<ResourceStaffJobDto>>> GetStaffJobsByStaffIdAsync(int staffId)
        {
            var url = string.Format(FpsApiEndpoints.GetResourceStaffJobs, staffId);
            var response = await _http.GetAsync<List<ResourceStaffJobRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceStaffJobDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceStaffJobDto>>>(response);
            return ApiResponseDto<List<ResourceStaffJobDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
