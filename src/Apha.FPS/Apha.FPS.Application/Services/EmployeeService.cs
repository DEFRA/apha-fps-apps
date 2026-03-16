using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<PaginatedResult<EmployeeDto>> GetFilteredEmployeesAsync(QueryParameters<string> queryFilter, int filterOption)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(queryFilter);
            string? filterValue = string.Empty;
            if (filterOption == 2)
            {
                filterValue = "T";
            }
            else if (filterOption == 3)
            {
                filterValue = "G";
            }
            var employees = await _employeeRepository.GetEmployeesByPrefixAsync(filter, filterValue);
            return _mapper.Map<PaginatedResult<EmployeeDto>>(employees);
        }

        public async Task<IEnumerable<EmployeeDto>> GetFilteredEmployeesAsync(int filterOption)
        {
            IEnumerable<Employee> employees;

            if (filterOption == 2)
            {
                employees = await _employeeRepository.GetEmployeesByPrefixAsync("T");
            }
            else if (filterOption == 3)
            {
                employees = await _employeeRepository.GetEmployeesByPrefixAsync("G");
            }
            else
            {
                employees = await _employeeRepository.GetAllEmployeesAsync();
            }

            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(string spNumber)
        {
            if (string.IsNullOrWhiteSpace(spNumber))
            {
                throw new ArgumentException("SPNumber cannot be null or empty.", nameof(spNumber));
            }

            var employee = await _employeeRepository.GetEmployeeByIdAsync(spNumber);
            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> AddEmployeeAsync(EmployeeDto employeeDto)
        {
            if (employeeDto == null)
            {
                throw new ArgumentNullException(nameof(employeeDto));
            }

            var employee = _mapper.Map<Employee>(employeeDto);
            var addedEmployee = await _employeeRepository.AddEmployeeAsync(employee);
            return _mapper.Map<EmployeeDto>(addedEmployee);
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto)
        {
            if (employeeDto == null)
            {
                throw new ArgumentNullException(nameof(employeeDto));
            }

            var employee = _mapper.Map<Employee>(employeeDto);
            var updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employee);
            return _mapper.Map<EmployeeDto>(updatedEmployee);
        }

        public async Task<bool> DeleteEmployeeAsync(string spNumber)
        {
            if (string.IsNullOrWhiteSpace(spNumber))
            {
                throw new ArgumentException("SPNumber cannot be null or empty.", nameof(spNumber));
            }

            return await _employeeRepository.DeleteEmployeeAsync(spNumber);
        }

        public async Task<IEnumerable<ManagerDto>> GetAllManagersAsync()
        {
            var managers = await _employeeRepository.GetAllManagersAsync();
            return _mapper.Map<IEnumerable<ManagerDto>>(managers);
        }
    }
}
