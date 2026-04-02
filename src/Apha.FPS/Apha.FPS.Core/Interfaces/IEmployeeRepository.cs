using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<PagedData<Employee>> GetEmployeesByPrefixAsync(PaginationParameters<string> query, string prefix);
        Task<IEnumerable<Employee>> GetEmployeesByPrefixAsync(string prefix);
        Task<Employee?> GetEmployeeByIdAsync(string spNumber);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task<Employee> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteEmployeeAsync(string spNumber);
        Task<IEnumerable<Manager>> GetAllManagersAsync();
        Task<IEnumerable<Manager>> GetAllPactManagersAsync();
    }
}
