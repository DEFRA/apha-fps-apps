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
        private readonly IFpsRequestContext _fpsYearContext;

        public EmployeeRepository(FpsDbContext dbContext, IFpsRequestContext fpsYearContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _fpsYearContext = fpsYearContext;
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
            queryEmployees = ApplyEmployeeFilter(queryEmployees, query.Filter);

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
            employee.FpsYear = _fpsYearContext.FpsYear;
            await _dbContext.Employees.AddAsync(employee);
            await _dbContext.SaveChangesAsync();

            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            employee.FpsYear = _fpsYearContext.FpsYear;
            _dbContext.Entry(employee).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(string spNumber)
        {
            var existingWgEmployee = await _dbContext.WgEmployees
                      .AsNoTracking()
                      .Where(e => e.SpNumber == spNumber && e.FpsYear == _fpsYearContext.FpsYear)
                      .FirstOrDefaultAsync();

            if (existingWgEmployee is not null)
                throw new InvalidOperationException(
                     $"Cannot delete SPNumber {spNumber} because linked Employee exist.");

            var employee = await _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.SPNumber == spNumber && e.FpsYear == _fpsYearContext.FpsYear)
                .FirstOrDefaultAsync();

            if (employee is null)
                throw new InvalidOperationException(
                    $"Employee with SPNumber {spNumber} does not exist.");                                           

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
                    staff.Name != null &&
                    !EF.Functions.ILike(staff.Name, "%general%") &&
                    !EF.Functions.ILike(staff.Name, "%vacancy%") &&
                    grade.GradeCode != null &&
                    grade.GradeCode.Length > 0 &&
                    grade.GradeCode.Substring(0, 1) != "G"
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

        public async Task<IEnumerable<Manager>> GetAllPactManagersAsync()
        {
            var query = (
                from grade in _dbContext.PactWorkGroupGradeViews
                join staff in _dbContext.StaffGeneralViews
                    on grade.WgGrade equals staff.WorkGroupGrade
                where
                    staff.Name != null &&
                    !EF.Functions.ILike(staff.Name, "%gen%") &&
                    !EF.Functions.ILike(staff.Name, "%vacancy%") &&
                    grade.GradeCode != null &&
                    (string.Compare(grade.GradeCode, "E") <= 0 || grade.GradeCode == "GD5")
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

            return await query.ToListAsync();
        }

        private static IQueryable<Employee> ApplyEmployeeFilter(IQueryable<Employee> queryEmployees, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryEmployees;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryEmployees;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("SPNumber", out var spNumber) && spNumber != null)
            {
                queryEmployees = queryEmployees.Where(x => x.SPNumber.Contains(spNumber.ToString()!));
            }

            if (dict.TryGetValue("FirstName", out var firstName) && firstName != null)
            {
                queryEmployees = queryEmployees.Where(x => x.FirstName!.Contains(firstName.ToString()!));
            }

            if (dict.TryGetValue("LastName", out var lastName) && lastName != null)
            {
                queryEmployees = queryEmployees.Where(x => x.LastName!.Contains(lastName.ToString()!));
            }

            if (dict.TryGetValue("Title", out var title) && title != null)
            {
                queryEmployees = queryEmployees.Where(x => x.Title!.Contains(title.ToString()!));
            }

            return queryEmployees;
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
                "fpscalyear" => ApplyOrder(query, i => i.FpsYear, descending),
                _ => query.OrderBy(e => e.SPNumber)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Employee> query, Expression<Func<Employee, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
