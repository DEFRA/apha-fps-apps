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
    public class PimsProjectCommentApiClient : IPimsProjectCommentApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsProjectCommentApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }


        public async Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            url = QueryStringHelper.AddQueryString(url, new { project, year, topic });
            var response = await _http.GetAsync<List<CommentRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<CommentDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<CommentDto>>>(response);
            return ApiResponseDto<List<CommentDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno)
        {
            var response = await _http.GetAsync<CommentRes>(string.Format(PimsApiEndpoints.GetCommentById, commentno));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<CommentDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
            return ApiResponseDto<CommentDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto)
        {
            CommentReq request = _mapper.Map<CommentReq>(dto);
            var response = await _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<CommentDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
            return ApiResponseDto<CommentDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto)
        {
            CommentReq request = _mapper.Map<CommentReq>(dto);
            var response = await _http.PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, commentno), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<CommentDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
            return ApiResponseDto<CommentDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno)
        {
            var response = await _http.DeleteAsync<bool>(string.Format(PimsApiEndpoints.DeleteComment, commentno));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync()
        {
            var response = await _http.GetAsync<List<CommentTopicRes>>(PimsApiEndpoints.GetCommentTopics);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(response);
            return ApiResponseDto<List<CommentTopicDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectCommentForecastSpendDto>> GetForecastSpendByProjectAsync(string project)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
            var response = await _http.GetAsync<ProjectCommentForecastSpendRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);
            return ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectCommentForecastSpendDto>> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
            ProjectCommentForecastSpendRes request = new() { ForecastSpend = forecastSpend };
            var response = await _http.PutAsync<ProjectCommentForecastSpendRes, ProjectCommentForecastSpendRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);
            return ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
