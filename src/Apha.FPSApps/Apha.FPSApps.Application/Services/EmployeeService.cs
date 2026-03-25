using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IFpsApiClient _fpsClient;

        public EmployeeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<EmployeeDto>>> GetFilteredEmployeesAsync(QueryParameters<string> criteria, int filterOption)
        {
            var employees = await _fpsClient.FpsEmployee.GetFilteredEmployeesAsync(criteria, filterOption);
            return employees;
        }

        public async Task<ApiResponseDto<EmployeeDto>> GetEmployeeByIdAsync(string spNumber)
        {
            var employee = await _fpsClient.FpsEmployee.GetEmployeeIdAsync(spNumber);
            return employee;
        }

        public async Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employee)
        {
            var result = await _fpsClient.FpsEmployee.CreateEmployeeAsync(employee);
            return result;
        }

        public async Task<ApiResponseDto<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto employee)
        {
            var result = await _fpsClient.FpsEmployee.UpdateEmployeeAsync(employee);
            return result;
        }

        public async Task<ApiResponseDto<bool>> DeleteEmployeeAsync(string spNumber)
        {
            var result = await _fpsClient.FpsEmployee.DeleteEmployeeAsync(spNumber);
            return result;
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllManagersAsync()
        {
            var managers = await _fpsClient.FpsEmployee.GetAllManagerAsync();
            return managers;
        }
    }
}
