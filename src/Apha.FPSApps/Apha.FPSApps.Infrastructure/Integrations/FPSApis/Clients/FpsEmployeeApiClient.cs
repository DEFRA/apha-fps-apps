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

        public async Task<ApiResponseDto<EmployeeDto>> GetEmployeeIdAsync(string spNumber)
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

        public async Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employee)
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

        public async Task<ApiResponseDto<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto employee)
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

        public async Task<ApiResponseDto<bool>> DeleteEmployeeAsync(string spNumber)
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

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllManagerAsync()
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

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllPactManagerAsync()
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

        public async Task<ApiResponseDto<List<WorkGroupPersonDto>>> GetAllWorkGroupPersonAsync()
        {
            var response = await _http.GetAsync<List<WorkGroupPersonRes>>(FpsApiEndpoints.GetAllPerson);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupPersonDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupPersonDto>>>(response);
            return ApiResponseDto<List<WorkGroupPersonDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<PaginatedResult<WorkGroupStaffDto>>> GetWorkGroupStaffAsync(QueryParameters<string> query, string? workGroup = null)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetWorkGroupStaffPaginated, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            var response = await _http.GetAsync<List<WorkGroupStaffRes>>(url);
            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<WorkGroupStaffDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<WorkGroupStaffDto>(
                    dto.Data ?? new List<WorkGroupStaffDto>(),
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<WorkGroupStaffDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<WorkGroupStaffDto>>>(response);
            return ApiResponseDto<PaginatedResult<WorkGroupStaffDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}
