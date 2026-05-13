using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectListApiClient : IPimsProjectListApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsProjectListApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsAsync(QueryParameters<string> query, int filterOption = 2)
        {
            
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAllProjects, query);
                url += $"&showWhichProjects={filterOption}";
                var response = await _http.GetAsync<List<ProjectListRes>>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);
                return ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(dto.Errors, dto.Meta);
            
        }

        public async Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsListAsync()
        {
          
                var response = await _http.GetAsync<List<ProjectListRes>>(PimsApiEndpoints.GetAllProjectsList);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);
                return ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(dto.Errors, dto.Meta);
            
        }

        public async Task<ApiResponseDto<ProjectDto>> GetFpsProjectByIdAsync(string parentproject)
        {
            try
            {
                var response = await _http.GetAsync<ProjectRes>(string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject));
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

        public async Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectByIdAsync(string parentproject)
        {
            try
            {
                var response = await _http.GetAsync<ProposedProjectRes>(string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject));
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

        public async Task<ApiResponseDto<List<ProjectsDto>>> GetYearlyDetailsByProjectAsync(string parentproject)
        {
            try
            {
                var response = await _http.GetAsync<List<ProjectsRes>>(string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, parentproject));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(response);
                return ApiResponseDto<List<ProjectsDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectsDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve yearly details", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> CreateProjectAsync(ProposedProjectDto dto)
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
        
    }
}
