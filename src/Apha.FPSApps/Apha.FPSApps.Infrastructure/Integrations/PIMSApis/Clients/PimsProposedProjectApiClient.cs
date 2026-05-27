using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProposedProjectApiClient : IPimsProposedProjectApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsProposedProjectApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> CreateProposedProjectAsync(ProposedProjectDto dto)
        {
            try
            {
                ProposedProjectReq request = _mapper.Map<ProposedProjectReq>(dto);
                var response = await _http.PostAsync<ProposedProjectReq, ProposedProjectRes>(PimsApiEndpoints.CreateProject, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);
                return ApiResponseDto<ProposedProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProposedProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetProjectProgramsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectPrograms);
                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve programs", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetProjectCustomersAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectCustomers);
                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve customers", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetProjectStatusesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetProjectStatuses);
                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve statuses", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
