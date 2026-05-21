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
    public class FpsDivisionGradeApiClient : IFpsDivisionGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsDivisionGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<DivisionGradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedDivisionGrades, query);
                var response = await _http.GetAsync<List<DivisionGradeRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<DivisionGradeDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<DivisionGradeDto>>>(response);
                    return ApiResponseDto<List<DivisionGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve paginated division grade data",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<DivisionGradeDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<DivisionGradeDto>> GetByIdAsync(string divisionGradeCode)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.GetDivisionGradeById, divisionGradeCode);
                var response = await _http.GetAsync<DivisionGradeRes>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<DivisionGradeDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<DivisionGradeDto>>(response);
                    return ApiResponseDto<DivisionGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = $"Failed to retrieve division grade '{divisionGradeCode}'",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<DivisionGradeDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<DivisionGradeDto>> CreateAsync(DivisionGradeDto dto)
        {
            try
            {
                var request = _mapper.Map<DivisionGradeReq>(dto);
                var response = await _http.PostAsync<DivisionGradeReq, DivisionGradeRes>(FpsApiEndpoints.CreateDivisionGrade, request);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<DivisionGradeDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<DivisionGradeDto>>(response);
                    return ApiResponseDto<DivisionGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to create division grade",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<DivisionGradeDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<DivisionGradeDto>> UpdateAsync(string originalCode, DivisionGradeDto dto)
        {
            try
            {
                var request = _mapper.Map<DivisionGradeReq>(dto);
                var url = string.Format(FpsApiEndpoints.UpdateDivisionGrade, originalCode);
                var response = await _http.PutAsync<DivisionGradeReq, DivisionGradeRes>(url, request);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<DivisionGradeDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<DivisionGradeDto>>(response);
                    return ApiResponseDto<DivisionGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = $"Failed to update division grade '{originalCode}'",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<DivisionGradeDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string divisionGradeCode)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.DeleteDivisionGrade, divisionGradeCode);
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
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = $"Failed to delete division grade '{divisionGradeCode}'",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetAllDivisionGrades);

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
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve all grade codes",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<string>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}
