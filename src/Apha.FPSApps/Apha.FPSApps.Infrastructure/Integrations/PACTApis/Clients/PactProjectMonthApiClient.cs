using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactProjectMonthApiClient : IPactProjectMonthApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactProjectMonthApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectMonthDto>>> GetProjectMonthByProjectAsync(string project)
        {
            var response = await _http.GetAsync<List<ProjectMonthRes>>(
                string.Format(PactApiEndpoints.GetProjectMonthsByProject, Uri.EscapeDataString(project)));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectMonthDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectMonthDto>>>(response);
            return ApiResponseDto<List<ProjectMonthDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectMonthDto>> GetProjectMonthAsync(string project, int monthNo)
        {
            var response = await _http.GetAsync<ProjectMonthRes>(
                string.Format(PactApiEndpoints.GetProjectMonthById, Uri.EscapeDataString(project), monthNo));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectMonthDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectMonthDto>>(response);
            return ApiResponseDto<ProjectMonthDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectMonthDto>> CreateProjectMonthAsync(ProjectMonthDto dto)
        {
            ProjectMonthReq request = _mapper.Map<ProjectMonthReq>(dto);

            var response = await _http.PostAsync<ProjectMonthReq, ProjectMonthRes>(
                PactApiEndpoints.CreateProjectMonth, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectMonthDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectMonthDto>>(response);
            return ApiResponseDto<ProjectMonthDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProjectMonthDto>> UpdateProjectMonthAsync(ProjectMonthDto dto)
        {
            ProjectMonthReq request = _mapper.Map<ProjectMonthReq>(dto);

            var response = await _http.PutAsync<ProjectMonthReq, ProjectMonthRes>(
                PactApiEndpoints.UpdateProjectMonth, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectMonthDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectMonthDto>>(response);
            return ApiResponseDto<ProjectMonthDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteProjectMonthAsync(string project, int monthNo)
        {
            var response = await _http.DeleteAsync<bool>(
                string.Format(PactApiEndpoints.DeleteProjectMonth, Uri.EscapeDataString(project), monthNo));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
