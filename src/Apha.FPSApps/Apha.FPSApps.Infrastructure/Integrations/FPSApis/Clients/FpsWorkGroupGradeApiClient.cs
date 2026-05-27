using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsWorkGroupGradeApiClient : IFpsWorkGroupGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsWorkGroupGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentre)
        {
            var url = string.Format(FpsApiEndpoints.GetWgGrades, Uri.EscapeDataString(profitCentre));
            var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            var url = string.Format(FpsApiEndpoints.DeleteWgGrade, Uri.EscapeDataString(wgGrade));
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}
