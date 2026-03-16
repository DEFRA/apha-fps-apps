using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        private readonly FpsDbContext _dbContext;

        public EmployeeRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _dbContext.Employees
                .AsNoTracking()
                .OrderBy(e => e.SPNumber)
                .ToListAsync();
        }

        public async Task<PagedData<Employee>> GetEmployeesByPrefixAsync(PaginationParameters<string> query, string prefix)
        {
            var queryEmployees = _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.SPNumber.StartsWith(prefix))
                .AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(query.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(query.Filter);
                if (filterModel != null)
                {
                    var dict = (IDictionary<string, object>)filterModel;

                    if (dict.ContainsKey("SPNumber") && dict["SPNumber"] != null)
                    {
                        queryEmployees = queryEmployees.Where(x => x.SPNumber.Contains(dict["SPNumber"].ToString()!));
                    }

                    if (dict.ContainsKey("FirstName") && dict["FirstName"] != null)
                    {
                        queryEmployees = queryEmployees.Where(x => x.FirstName!.Contains(dict["FirstName"].ToString()!));
                    }

                    if (dict.ContainsKey("LastName") && dict["LastName"] != null)
                    {
                        queryEmployees = queryEmployees.Where(x => x.LastName!.Contains(dict["LastName"].ToString()!));
                    }

                    if (dict.ContainsKey("Title") && dict["Title"] != null)
                    {
                        queryEmployees = queryEmployees.Where(x => x.Title!.Contains(dict["Title"].ToString()!));
                    }

                    if (dict.ContainsKey("FPSCalYear") && dict["FPSCalYear"] != null)
                    {
                        queryEmployees = queryEmployees.Where(x => x.FPSCalYear.ToString() == dict["FPSCalYear"].ToString());
                    }
                }
            }

            // Apply sorting
            queryEmployees = (IQueryable<Employee>)ApplySorting(queryEmployees, query.SortBy, query.Descending);

            // Execute query
            var result = await queryEmployees.ToListAsync();

            // Apply paging
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPrefixAsync(string prefix)
        {
            return await _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.SPNumber.StartsWith(prefix))
                .OrderBy(e => e.SPNumber)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string spNumber)
        {
            return await _dbContext.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.SPNumber == spNumber);
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            await _dbContext.Employees.AddAsync(employee);
            await _dbContext.SaveChangesAsync();

            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            _dbContext.Entry(employee).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(string spNumber)
        {  
            var employee = await _dbContext.Employees.FindAsync(spNumber);

            if (employee == null)
            {
                return false;
            }

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Manager>> GetAllManagersAsync()
        {
            var query = (
                 from staff in _dbContext.StaffActiveView
                 join grade in _dbContext.WorkgroupGradeGeneralView
                     on staff.WorkgroupGrade equals grade.WgGrade
                 where
                     !string.IsNullOrEmpty(staff.Name) &&
                     !staff.Name.ToLower().Contains("general") &&
                     !staff.Name.ToLower().Contains("vacancy") &&
                     !string.IsNullOrEmpty(grade.GradeCode) &&
                     !grade.GradeCode.StartsWith('G')
                 select new Manager
                 {
                     Name = staff.Name,
                     WorkGroup = grade.WorkGroup,
                     GradeCode = grade.GradeCode,
                     Expr1 = grade.GradeCode!.Substring(0, 1)
                 }
             )
             .Distinct()
             .OrderBy(x => x.Name);
            
            var managers = await query.ToListAsync();
            return managers;
        }

        private static IQueryable ApplySorting(IQueryable<Employee> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.SPNumber);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<Employee> query, string property, bool descending)
        {
            return property switch
            {
                "spnumber" => ApplyOrder(query, i => i.SPNumber, descending),
                "firstname" => ApplyOrder(query, i => i.FirstName, descending),
                "lastname" => ApplyOrder(query, i => i.LastName, descending),
                "title" => ApplyOrder(query, i => i.Title, descending),
                "fpscalyear" => ApplyOrder(query, i => i.FPSCalYear, descending),
                _ => query.OrderBy(e => e.SPNumber)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Employee> query, Expression<Func<Employee, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
