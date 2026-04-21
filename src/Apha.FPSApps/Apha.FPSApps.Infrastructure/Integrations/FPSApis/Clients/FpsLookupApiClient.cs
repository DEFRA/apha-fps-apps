using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsLookupApiClient : IFpsLookupApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsLookupApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<StatusDto>>> GetAllStatusesAsync()
        {
            var response = await _http.GetAsync<List<StatusRes>>(FpsApiEndpoints.GetAllStatuses);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StatusDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<StatusDto>>>(response);
            return ApiResponseDto<List<StatusDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync()
        {
            var response = await _http.GetAsync<List<DiseaseRes>>(FpsApiEndpoints.GetAllDiseases);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(response);
            return ApiResponseDto<List<DiseaseDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync()
        {
            var response = await _http.GetAsync<List<CustomerRes>>(FpsApiEndpoints.GetAllCustomers);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<CustomerDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<CustomerDto>>>(response);
            return ApiResponseDto<List<CustomerDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ContractDto>>> GetAllContractsAsync()
        {
            var response = await _http.GetAsync<List<ContractRes>>(FpsApiEndpoints.GetAllContracts);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);
            return ApiResponseDto<List<ContractDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProjectGroupRes>>(FpsApiEndpoints.GetAllProjectGroups);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);
                return ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project groups", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
