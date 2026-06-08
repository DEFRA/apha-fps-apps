using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProfitCentreGradeApiClient : IFpsProfitCentreGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProfitCentreGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre)
        {
            var baseUrl = string.Format(FpsApiEndpoints.GetPcGrades, Uri.EscapeDataString(profitCentre));
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            var response = await _http.GetAsync<List<ProfitCentreGradeRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(response);
                return ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedPcGrades, query);
            var response = await _http.GetAsync<List<ProfitCentreGradeRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(response);
                return ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProfitCentreGradeDto>> GetByIdAsync(string pcGrade)
        {
            var url = string.Format(FpsApiEndpoints.GetPcGradeById, Uri.EscapeDataString(pcGrade));
            var response = await _http.GetAsync<ProfitCentreGradeRes>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProfitCentreGradeDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreGradeDto>>(response);
                return ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProfitCentreGradeDto>> CreateAsync(ProfitCentreGradeDto dto)
        {
            var request = _mapper.Map<ProfitCentreGradeReq>(dto);
            var response = await _http.PostAsync<ProfitCentreGradeReq, ProfitCentreGradeRes>(FpsApiEndpoints.CreatePcGrade, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProfitCentreGradeDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreGradeDto>>(response);
                return ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProfitCentreGradeDto>> UpdateAsync(string originalPcGrade, ProfitCentreGradeDto dto)
        {
            var request = _mapper.Map<ProfitCentreGradeReq>(dto);
            var url = string.Format(FpsApiEndpoints.UpdatePcGrade, Uri.EscapeDataString(originalPcGrade));
            var response = await _http.PutAsync<ProfitCentreGradeReq, ProfitCentreGradeRes>(url, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProfitCentreGradeDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreGradeDto>>(response);
                return ApiResponseDto<ProfitCentreGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string pcGrade)
        {
            var url = string.Format(FpsApiEndpoints.DeletePcGrade, Uri.EscapeDataString(pcGrade));
            var response = await _http.DeleteAsync<bool?>(url);

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

        public async Task<ApiResponseDto<List<string>>> GetAllProfitCentreCodesAsync()
        {
            var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetPcGradeProfitCentres);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetAllPcGradesAsync()
        {
            var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetAllPcGrades);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
