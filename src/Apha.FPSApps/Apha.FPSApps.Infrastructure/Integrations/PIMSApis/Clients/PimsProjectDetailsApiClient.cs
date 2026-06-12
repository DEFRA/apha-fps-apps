using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectDetailsApiClient : IPimsProjectDetailsApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsProjectDetailsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<ProjectDetailDto>> GetPimsDetailAsync(string parentproject)
        {
            try
            {
                var response = await _http.GetAsync<ProjectDetailRes>(string.Format(PimsApiEndpoints.GetPimsDetail, parentproject));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);
                return ApiResponseDto<ProjectDetailDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDetailDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve PIMS detail", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDetailDto>> SavePimsDetailAsync(string parentproject, ProjectDetailDto dto)
        {
            try
            {
                ProjectDetailReq request = _mapper.Map<ProjectDetailReq>(dto);
                var response = await _http.PostAsync<ProjectDetailReq, ProjectDetailRes>(
                    string.Format(PimsApiEndpoints.SavePimsDetail, parentproject), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);
                return ApiResponseDto<ProjectDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDetailDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to save PIMS detail", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectAsync(string parentproject)
        {
            try
            {
                var response = await _http.GetAsync<ProposedProjectRes>(string.Format(PimsApiEndpoints.GetProposedProject, parentproject));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);
                return ApiResponseDto<ProposedProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProposedProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve proposed project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> UpdateProposedProjectAsync(string parentproject, ProposedProjectDto dto)
        {
            try
            {
                ProposedProjectReq request = _mapper.Map<ProposedProjectReq>(dto);
                var response = await _http.PutAsync<ProposedProjectReq, ProposedProjectRes>(
                    string.Format(PimsApiEndpoints.UpdateProposedProject, parentproject), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);
                return ApiResponseDto<ProposedProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProposedProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update proposed project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<RiskDto>>> GetAllRiskAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks);
                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<RiskDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<RiskDto>>>(response);
                return ApiResponseDto<List<RiskDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<RiskDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve risk ratings", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<YearDto>>> GetAllYearAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<YearRes>>(PimsApiEndpoints.GetAllYears);
                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<YearDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<YearDto>>>(response);
                return ApiResponseDto<List<YearDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<YearDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve years", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> GetFpsProjectAsync(string parentproject)
        {
            try
            {
                var response = await _http.GetAsync<ProjectRes>(string.Format(PimsApiEndpoints.GetFpsProjectByProjectDetails, parentproject));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve FPS project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
