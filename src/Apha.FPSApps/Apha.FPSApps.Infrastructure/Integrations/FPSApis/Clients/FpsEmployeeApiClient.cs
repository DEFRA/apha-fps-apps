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
    public class FpsEmployeeApiClient : IFpsEmployeeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsEmployeeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<EmployeeDto>>> GetFilteredEmployeesAsync(QueryParameters<string> criteria, int filterOption)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetFilteredEmployees, filterOption), criteria);
                var response = await _http.GetAsync<List<EmployeeRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<EmployeeDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<EmployeeDto>>>(response);
                    return ApiResponseDto<List<EmployeeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to retrieve employees",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<EmployeeDto>>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<EmployeeDto>> GetEmployeeIdAsync(string spNumber)
        {
            try
            {
                var response = await _http.GetAsync<EmployeeRes>(string.Format(FpsApiEndpoints.GetEmployeeById, spNumber));

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<EmployeeDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<EmployeeDto>>(response);
                    return ApiResponseDto<EmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to retrieve employee",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<EmployeeDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employee)
        {
            try
            {
                var employeeReq = _mapper.Map<EmployeeReq>(employee);
                var response = await _http.PostAsync<EmployeeReq, EmployeeRes>(FpsApiEndpoints.CreateEmployee, employeeReq);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<EmployeeDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<EmployeeDto>>(response);
                    return ApiResponseDto<EmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to create employee",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<EmployeeDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto employee)
        {
            try
            {
                var employeeReq = _mapper.Map<EmployeeReq>(employee);
                var response = await _http.PutAsync<EmployeeReq, EmployeeRes>(FpsApiEndpoints.UpdateEmployee, employeeReq);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<EmployeeDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<EmployeeDto>>(response);
                    return ApiResponseDto<EmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to update employee",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<EmployeeDto>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteEmployeeAsync(string spNumber)
        {
            try
            {
                var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteEmployee, spNumber));

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
                        Message = "Failed to delete employee",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllManagerAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ManagerRes>>(FpsApiEndpoints.GetAllManagers);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
                    return ApiResponseDto<List<ManagerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to retrieve managers",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<ManagerDto>>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllPactManagerAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ManagerRes>>(FpsApiEndpoints.GetAllPactManagers);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
                    return ApiResponseDto<List<ManagerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to retrieve managers",
                        Code = internalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<ManagerDto>>.FailureResponse(apiErrosDto, new ApiMetaDto());
            }
        }
    }
}
