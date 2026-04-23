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
    /// <summary>
    /// HTTP client implementation for Division API operations.
    /// </summary>
    public class FpsDivisionApiClient : IFpsDivisionApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsDivisionApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<IEnumerable<DivisionDto>>> GetAllDivisionsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<DivisionRes>>(FpsApiEndpoints.GetAllDivisions);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<DivisionDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<DivisionDto>>>(response);
                    return ApiResponseDto<IEnumerable<DivisionDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve division data",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<IEnumerable<DivisionDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<DivisionDto>>> GetAllDivisionsPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedDivisions, query);
                var response = await _http.GetAsync<List<DivisionRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<DivisionDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<DivisionDto>>>(response);
                    return ApiResponseDto<List<DivisionDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve paginated division data",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<DivisionDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<DivisionDto>> GetDivisionByNameAsync(string divName)
        {
            try
            {
                var response = await _http.GetAsync<DivisionRes>(string.Format(FpsApiEndpoints.GetDivisionByName, divName));

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<DivisionDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<DivisionDto>>(response);
                    return ApiResponseDto<DivisionDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = $"Failed to retrieve division '{divName}'",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<DivisionDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<DivisionDto>> CreateDivisionAsync(DivisionDto divisionDto)
        {
            try
            {
                var request = _mapper.Map<DivisionReq>(divisionDto);
                var response = await _http.PostAsync<DivisionReq, DivisionRes>(FpsApiEndpoints.CreateDivision, request);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<DivisionDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<DivisionDto>>(response);
                    return ApiResponseDto<DivisionDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to create division",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<DivisionDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<DivisionDto>> UpdateDivisionAsync(string divName, DivisionDto divisionDto)
        {
            try
            {
                var request = _mapper.Map<DivisionReq>(divisionDto);
                var response = await _http.PutAsync<DivisionReq, DivisionRes>(string.Format(FpsApiEndpoints.UpdateDivision, divName), request);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<DivisionDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<DivisionDto>>(response);
                    return ApiResponseDto<DivisionDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = $"Failed to update division '{divName}'",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<DivisionDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteDivisionAsync(string divName)
        {
            try
            {
                var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteDivision, divName));

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
                        Message = $"Failed to delete division '{divName}'",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}
