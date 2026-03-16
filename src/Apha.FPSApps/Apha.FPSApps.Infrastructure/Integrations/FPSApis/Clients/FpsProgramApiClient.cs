using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Net.Http.Headers;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProgramApiClient : IFpsProgramApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";
        private const string programApiEndpoint = "api/program";

        public FpsProgramApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsAsync()
        {
            try
            {
                var response = await _http.GetAsync<IEnumerable<ProgramDto>>(programApiEndpoint);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(response);
                    return ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to retrieve programs",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(programApiEndpoint+"/paged", query);
                var response = await _http.GetAsync<List<ProgramDto>>(url);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
                    return ApiResponseDto<List<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to retrieve paginated programs",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<List<ProgramDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProgramDto?>> GetProgramByIdAsync(string programNo)
        {
            try
            {
                var response = await _http.GetAsync<ProgramDto>($"{programApiEndpoint}/{programNo}");
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<ProgramDto?>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<ProgramDto?>>(response);
                    return ApiResponseDto<ProgramDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to retrieve program",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<ProgramDto?>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string?>>> GetAllDirectoratesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string?>>($"{programApiEndpoint}/directorates");
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<string?>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<string?>>>(response);
                    return ApiResponseDto<List<string?>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to retrieve directorates",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<List<string?>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProgramDto>> AddProgramAsync(ProgramDto programDto)
        {
            try
            {
                var response = await _http.PostAsync<ProgramDto, ProgramDto>(programApiEndpoint, programDto);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<ProgramDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<ProgramDto>>(response);
                    return ApiResponseDto<ProgramDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to add program",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<ProgramDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProgramDto>> UpdateProgramAsync(ProgramDto programDto)
        {
            try
            {
                var response = await _http.PutAsync<ProgramDto, ProgramDto>(programApiEndpoint, programDto);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<ProgramDto>>(response);
                }
                else
                {
                    var reponseDto = _mapper.Map<ApiResponseDto<ProgramDto>>(response);
                    return ApiResponseDto<ProgramDto>.FailureResponse(reponseDto.Errors, reponseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to update program",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<ProgramDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProgramAsync(string programNo)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>($"{programApiEndpoint}/{programNo}");
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
                var apiErrorsDto = new List<ApiErrorDto> {
                        new ApiErrorDto {
                            Message = "Failed to delete program",
                            Code = internalCodeError,
                            Details = null
                        }
                    };
                return ApiResponseDto<bool>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}