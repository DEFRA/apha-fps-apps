using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Reflection.Emit;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsStaffJobApiClient : IFpsStaffJobApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsStaffJobApiClient(IFpsHttpExecutor http, IMapper mapper)  
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobAsync(QueryParameters<string> staffJob, string jobCode)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"api/staffjob?jobCode={jobCode}", staffJob);
                var response = await _http.GetAsync<List<StaffJobViewRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
                    return ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve staff jobs",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync()
        {
            try
            {
                var response = await _http.GetAsync<IEnumerable<StaffWorkgroupLookupRes>>("api/staffjob/workgrouplookup");

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(response);
                    return ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                         Message = "Failed to retrieve workgroup lookup data",
                         Code = internalCodeError,
                         Details = null
                     }
                 };
                return ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<decimal?>> GetStaffChargeRate(string staffId, string jobcode)
        {
            try
            {
                var response = await _http.GetAsync<decimal?>($"api/staffjob/chargerate?staffId={staffId}&jobcode={jobcode}");

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<decimal?>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<decimal?>>(response);
                    return ApiResponseDto<decimal?>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve staff charge rate",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<decimal?>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId, string jobCode)
        {
            try
            {
                var response = await _http.GetAsync<StaffJobRes>($"api/staffjob/{staffId}/{jobCode}");

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                    return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve staff job",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob)
        {
            try
            {
                var staffJobReq = _mapper.Map<StaffJobReq>(staffJob);
                var response = await _http.PostAsync<StaffJobReq, StaffJobRes>("api/staffjob", staffJobReq);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                    return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to create staff job",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(StaffJobDto staffJob)
        {
            try
            {
                var staffJobReq = _mapper.Map<StaffJobReq>(staffJob);
                var response = await _http.PutAsync<StaffJobReq, StaffJobRes>("api/staffjob", staffJobReq);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                    return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to update staff job",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>($"api/staffjob?staffId={staffId}&jobcode={jobCode}");

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
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to delete staff job",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<StaffJobViewDto?>> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            try
            {
                var response = await _http.GetAsync<StaffJobViewRes>($"api/staffjob/view?staffId={staffId}&jobcode={jobCode}");

                if (response.Success)
                {
                    var mappedData = response.Data != null ? _mapper.Map<StaffJobViewDto>(response.Data) : null;
                    return ApiResponseDto<StaffJobViewDto?>.SuccessResponse(mappedData);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<StaffJobViewDto?>>(response);
                    return ApiResponseDto<StaffJobViewDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve staff job view",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<StaffJobViewDto?>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }
    }
}
