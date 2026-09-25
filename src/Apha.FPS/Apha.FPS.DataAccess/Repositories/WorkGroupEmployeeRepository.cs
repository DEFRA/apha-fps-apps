using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WorkGroupEmployeeRepository : BaseRepository, IWorkGroupEmployeeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupEmployeeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _dbContext.WorkGroupEmployees
                .AsNoTracking()
                .Where(wg => wg.PactId == pactId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
                        TimeRecorder = wg.TimeRecorder,
                        StartDate = wg.StartDate,
                        EndDate = wg.EndDate,
                        HoursPerWeek = wg.HoursPerWeek,
                    })
                .FirstOrDefaultAsync(default);
        }

        public async Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdForStaffAsync(string pactId)
        {
            return await _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(wg => wg.PactId == pactId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
                        TimeRecorder = wg.TimeRecorder,
                        StartDate = wg.StartDate,
                        EndDate = wg.EndDate,
                        HoursPerWeek = wg.HoursPerWeek,
                        FpsYear = wg.FpsYear,
                        UserId = wg.UserId,
                        Dt2Username = wg.Dt2Username,
                        UserEmail = wg.UserEmail
                    })
                .FirstOrDefaultAsync(default);
        }

        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId);
            if (existing == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{entity.PactId}' was not found.");

            existing.HrsPaid = entity.HrsPaid;
            existing.Leave = entity.Leave;
            existing.SickSpecial = entity.SickSpecial;
            existing.HrsAvail = entity.HrsPaid - (entity.Leave + entity.SickSpecial);
            existing.PersonStatus = entity.PersonStatus;
            existing.PersonClass = entity.PersonClass;
            if (entity.MakeAvailable == 1)
            {
                entity.MakeAvailable = -1;
            }
            existing.MakeAvailable = entity.MakeAvailable;

            await _dbContext.SaveChangesAsync(default);
            return existing;
        }

        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId);
            if (existing == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{entity.PactId}' was not found.");

            existing.SpNumber = entity.SpNumber;
            existing.WorkGroupGrade = entity.WorkGroupGrade;
            existing.HrsPaid = entity.HrsPaid;
            existing.Leave = entity.Leave;
            existing.SickSpecial = entity.SickSpecial;
            existing.HrsAvail = entity.HrsAvail;
            existing.PersonStatus = entity.PersonStatus;
            existing.PersonClass = entity.PersonClass;
            if (entity.MakeAvailable == 1)
            {
                entity.MakeAvailable = -1;
            }
            existing.MakeAvailable = entity.MakeAvailable;
            if (entity.TimeRecorder == 1)
            {
                entity.TimeRecorder = -1;
            }
            existing.TimeRecorder = entity.TimeRecorder;
            existing.StartDate = entity.StartDate;
            existing.EndDate = entity.EndDate;
            existing.HoursPerWeek = entity.HoursPerWeek;

            await _dbContext.SaveChangesAsync(default);
            return existing;
        }

        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            var all = _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(x => x.WorkGroupGrade == wgGrade
                         && x.PersonStatus != "I"
                         && x.UserEmail != null && EF.Functions.ILike(x.UserEmail, _requestContext.UserEmailId))
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
                        TimeRecorder = wg.TimeRecorder,
                        StartDate = wg.StartDate,
                        EndDate = wg.EndDate,
                        HoursPerWeek = wg.HoursPerWeek,
                        FpsYear = wg.FpsYear,
                        UserId = wg.UserId,
                        Dt2Username = wg.Dt2Username,
                        UserEmail = wg.UserEmail,
                    });

            var filtered = ApplyFilter(all, query.Filter);
            var sorted   = ApplySorting(filtered, query.SortBy, query.Descending);

            return await ApplyPagingAsync(sorted, query.Page, query.PageSize);
        }

        public async Task<PagedData<WorkGroupEmployeeView>> GetAllActiveWorkGroupEmployeesAsync(
            PaginationParameters<string> query, string wgGrade)
        {
            // CA1862 suppressed: this is an EF Core query translated to SQL. The
            // string.Equals(StringComparison) overload cannot be translated by Npgsql,
            // whereas ToUpper() maps to SQL UPPER().
#pragma warning disable CA1862
            var workGroupEmployeeQuery = _dbContext.WorkGroupEmployees
                .AsNoTracking()
                .Where(wg => wg.WorkGroupGrade == wgGrade && wg.PersonStatus.ToUpper() != "I")
#pragma warning restore CA1862
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new
                    {
                        wg.PactId,
                        wg.SpNumber,
                        wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        wg.PersonStatus,
                        wg.PersonClass,
                        wg.HrsPaid,
                        wg.Leave,
                        wg.SickSpecial,
                        wg.HrsAvail,
                        wg.MakeAvailable,
                        wg.TimeRecorder,
                        wg.StartDate,
                        wg.EndDate,
                        wg.HoursPerWeek
                    })
                .Distinct()
                .Select(x => new WorkGroupEmployeeView
                {
                    PactId = x.PactId,
                    SpNumber = x.SpNumber,
                    WorkGroupGrade = x.WorkGroupGrade,
                    Name = x.Name,
                    PersonStatus = x.PersonStatus,
                    PersonClass = x.PersonClass,
                    HrsPaid = x.HrsPaid,
                    Leave = x.Leave,
                    SickSpecial = x.SickSpecial,
                    HrsAvail = x.HrsAvail,
                    MakeAvailable = x.MakeAvailable,
                    TimeRecorder = x.TimeRecorder,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    HoursPerWeek = x.HoursPerWeek
                })
                .AsQueryable();

            workGroupEmployeeQuery = ApplyFilter(workGroupEmployeeQuery, query.Filter);
            workGroupEmployeeQuery = ApplySorting(workGroupEmployeeQuery, query.SortBy, query.Descending);

            var result = await workGroupEmployeeQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<bool> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            var entity = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == pactId);
            if (entity == null)
                return false;

            _dbContext.WorkGroupEmployees.Remove(entity);
            await _dbContext.SaveChangesAsync(default);
            return true;
        }

        public async Task<bool> HasAssociatedStaffAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return false;

            return await _dbContext.WorkGroupEmployees
                .AnyAsync(e => e.WorkGroupGrade == wgGrade);
        }

        public async Task<bool> HasAssociatedMonthlyTimeAsync(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
                return false;

            return await _dbContext.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(mt => mt.PactStaffId == pactId);
        }

        public async Task<string> GetNextPactIdAsync()
        {
            var pactIds = await _dbContext.WorkGroupEmployees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(e => e.FpsYear == _requestContext.FpsYear)
                .Select(e => e.PactId)
                .ToListAsync(default);

            var maxNumeric = pactIds
                .Select(id => int.TryParse(id, out var n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .DefaultIfEmpty(0)
                .Max();

            return (maxNumeric + 1).ToString();
        }

        public async Task<WorkGroupEmployee> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;
            if (string.IsNullOrWhiteSpace(entity.PactId))
            {
                entity.PactId = await GetNextPactIdAsync();
            }
            if (entity.MakeAvailable==1)
            {
                entity.MakeAvailable = -1;
            }
            if (entity.TimeRecorder==1)
            {
                entity.TimeRecorder = -1;
            }
            await _dbContext.WorkGroupEmployees.AddAsync(entity);
            await _dbContext.SaveChangesAsync(default);
            return entity;
        }

       
        private static IQueryable<WorkGroupEmployeeView> ApplyFilter(IQueryable<WorkGroupEmployeeView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("PactId", out var pactId) && pactId != null)
                query = query.Where(x => x.PactId != null && EF.Functions.ILike(x.PactId, $"%{pactId}%"));

            if (dict.TryGetValue("SpNumber", out var spNumber) && spNumber != null)
                query = query.Where(x => x.SpNumber != null && EF.Functions.ILike(x.SpNumber, $"%{spNumber}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{name}%"));

            if (dict.TryGetValue("WorkGroupGrade", out var workGroupGrade) && workGroupGrade != null)
                query = query.Where(x => x.WorkGroupGrade != null && EF.Functions.ILike(x.WorkGroupGrade, $"%{workGroupGrade}%"));

            // PersonStatus is a fixed two-value code (A/I), so it is matched exactly rather
            // than with %...% wildcards - a "contains" match would make "A" also match nothing
            // meaningful and would blur the two codes if more statuses are added later.
            if (dict.TryGetValue("PersonStatus", out var personStatus) && personStatus != null)
                query = query.Where(x => x.PersonStatus != null && EF.Functions.ILike(x.PersonStatus, $"{personStatus}"));

            if (dict.TryGetValue("PersonClass", out var personClass) && personClass != null)
                query = query.Where(x => x.PersonClass != null && EF.Functions.ILike(x.PersonClass, $"%{personClass}%"));

            // Numeric hours columns match on exact value, mirroring the Price filter in
            // TestRequirementRCCostRepository. A non-numeric or partially typed entry fails
            // to parse and is ignored rather than emptying the grid.
            query = ApplyNumericFilter(dict, "HrsPaid", query, x => x.HrsPaid);
            query = ApplyNumericFilter(dict, "Leave", query, x => x.Leave);
            query = ApplyNumericFilter(dict, "SickSpecial", query, x => x.SickSpecial);
            query = ApplyNumericFilter(dict, "HrsAvail", query, x => x.HrsAvail);
            query = ApplyNumericFilter(dict, "HoursPerWeek", query, x => x.HoursPerWeek);

            // The grid renders these as checkbox dropdowns posting "true"/"false", but the
            // database stores the flags as -1 (true) and 0 (false), so the parsed bool is
            // mapped to the stored code and compared exactly.
            query = ApplyFlagFilter(dict, "MakeAvailable", query, x => x.MakeAvailable);
            query = ApplyFlagFilter(dict, "TimeRecorder", query, x => x.TimeRecorder);

            // Date columns are matched on the calendar day only, ignoring any time component
            // stored against the row.
            query = ApplyDateFilter(dict, "StartDate", query, x => x.StartDate);
            query = ApplyDateFilter(dict, "EndDate", query, x => x.EndDate);

            return query;
        }

        // The grid's native date picker posts ISO yyyy-MM-dd, but the filter value can also
        // arrive as UK dd/MM/yyyy (typed entry or a browser using the UK locale) and Newtonsoft
        // may already have boxed it as a DateTime. Each of those shapes is accepted here so the
        // filter is not silently ignored.
        private static readonly string[] DateFilterFormats =
        [
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "dd.MM.yyyy",
            "dd/MM/yy",
            "d/M/yy"
        ];

        /// <summary>
        /// Parses a grid filter value into a calendar date without ever consulting the
        /// ambient culture. The API host culture is not guaranteed to be en-GB, so
        /// DateTime.TryParse would read "05/06/2025" as 6 May on an en-US host while the
        /// grid means 5 June. TryParseExact against an explicit, ordered format list makes
        /// the result deterministic: ISO values (produced by the date picker) are matched
        /// first, then the UK day-first formats a user can type.
        /// </summary>
        private static bool TryParseFilterDateInvariant(object rawValue, out DateTime value)
        {
            value = default;

            if (rawValue is DateTime dateTimeValue)
            {
                value = dateTimeValue;
                return true;
            }

            if (rawValue is DateTimeOffset dateTimeOffsetValue)
            {
                value = dateTimeOffsetValue.DateTime;
                return true;
            }

            var text = rawValue?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string[] formats =
            [
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
                "yyyy/MM/dd",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-M-yyyy",
                "dd.MM.yyyy",
                "d.M.yyyy"
            ];

            return DateTime.TryParseExact(
                text,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value);
        }

        private static IQueryable<WorkGroupEmployeeView> ApplyDateFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<WorkGroupEmployeeView> query,
            Expression<Func<WorkGroupEmployeeView, DateTime?>> selector)
        {
            if (!dict.TryGetValue(key, out var rawValue) || rawValue == null)
                return query;

            if (!TryParseFilterDateInvariant(rawValue, out var value))
                return query;

            // The columns are mapped to 'timestamp with time zone', so Npgsql only accepts
            // DateTime values whose Kind is Utc. The filter text is a calendar day, so it is
            // treated as a UTC day and matched with a half-open [day, day + 1) range. This
            // keeps the comparison culture independent (both sides are UTC instants) and
            // avoids translating DateTime.Date on a timestamptz column.
            var dayStartUtc = DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
            var dayEndUtc = dayStartUtc.AddDays(1);

            var propertyValue = Expression.Property(selector.Body, nameof(Nullable<DateTime>.Value));
            var hasValue = Expression.Property(selector.Body, nameof(Nullable<DateTime>.HasValue));

            var inRange = Expression.AndAlso(
                Expression.GreaterThanOrEqual(propertyValue, Expression.Constant(dayStartUtc, typeof(DateTime))),
                Expression.LessThan(propertyValue, Expression.Constant(dayEndUtc, typeof(DateTime))));

            var predicate = Expression.Lambda<Func<WorkGroupEmployeeView, bool>>(
                Expression.AndAlso(hasValue, inRange),
                selector.Parameters);

            return query.Where(predicate);
        }

        private static IQueryable<WorkGroupEmployeeView> ApplyFlagFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<WorkGroupEmployeeView> query,
            Expression<Func<WorkGroupEmployeeView, int?>> selector)
        {
            if (!dict.TryGetValue(key, out var rawValue) || rawValue == null)
                return query;

            if (!bool.TryParse(rawValue.ToString(), out var value))
                return query;

            var storedValue = value ? -1 : 0;

            var predicate = Expression.Lambda<Func<WorkGroupEmployeeView, bool>>(
                Expression.Equal(selector.Body, Expression.Constant(storedValue, typeof(int?))),
                selector.Parameters);

            return query.Where(predicate);
        }

        private static IQueryable<WorkGroupEmployeeView> ApplyNumericFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<WorkGroupEmployeeView> query,
            Expression<Func<WorkGroupEmployeeView, double?>> selector)
        {
            if (!dict.TryGetValue(key, out var rawValue) || rawValue == null)
                return query;

            if (!double.TryParse(rawValue.ToString(), out var value))
                return query;

            var predicate = Expression.Lambda<Func<WorkGroupEmployeeView, bool>>(
                Expression.Equal(selector.Body, Expression.Constant(value, typeof(double?))),
                selector.Parameters);

            return query.Where(predicate);
        }

        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeForStaffAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            // CA1862 suppressed: this is an EF Core query translated to SQL. The
            // string.Equals(StringComparison) overload cannot be translated by Npgsql,
            // whereas ToLower() maps to SQL LOWER().
#pragma warning disable CA1862
            var workGroupEmployeeQuery = _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(wg => (string.IsNullOrWhiteSpace(wgGrade) || wg.WorkGroupGrade == wgGrade)
                          && wg.UserEmail != null
                          && wg.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower())
#pragma warning restore CA1862
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new
                    {
                        wg.PactId,
                        wg.SpNumber,
                        wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        wg.PersonStatus,
                        wg.PersonClass,
                        wg.HrsPaid,
                        wg.Leave,
                        wg.SickSpecial,
                        wg.HrsAvail,
                        wg.MakeAvailable,
                        wg.TimeRecorder,
                        wg.StartDate,
                        wg.EndDate,
                        wg.HoursPerWeek,
                        wg.FpsYear,
                        wg.UserId,
                        wg.Dt2Username,
                        wg.UserEmail
                    })
                .Distinct()
                .Select(x => new WorkGroupEmployeeView
                {
                    PactId = x.PactId,
                    SpNumber = x.SpNumber,
                    WorkGroupGrade = x.WorkGroupGrade,
                    Name = x.Name,
                    PersonStatus = x.PersonStatus,
                    PersonClass = x.PersonClass,
                    HrsPaid = x.HrsPaid,
                    Leave = x.Leave,
                    SickSpecial = x.SickSpecial,
                    HrsAvail = x.HrsAvail,
                    MakeAvailable = x.MakeAvailable,
                    TimeRecorder = x.TimeRecorder,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    HoursPerWeek = x.HoursPerWeek,
                    FpsYear = x.FpsYear,
                    UserId = x.UserId,
                    Dt2Username = x.Dt2Username,
                    UserEmail = x.UserEmail
                })
                .AsQueryable();

            workGroupEmployeeQuery = ApplyFilter(workGroupEmployeeQuery, query.Filter);
            workGroupEmployeeQuery = ApplySorting(workGroupEmployeeQuery, query.SortBy, query.Descending);

            var result = await workGroupEmployeeQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

           

        private static IQueryable<WorkGroupEmployeeView> ApplySorting(IQueryable<WorkGroupEmployeeView> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "pactid" => descending
                    ? query.OrderByDescending(x => x.PactId!.Length).ThenByDescending(x => x.PactId)
                    : query.OrderBy(x => x.PactId!.Length).ThenBy(x => x.PactId),
                "sickspecial" => ApplyOrder(query, x => x.SickSpecial, descending),
                "hrspaid" => ApplyOrder(query, x => x.HrsPaid, descending),
                "leave" => ApplyOrder(query, x => x.Leave, descending),
                "makeavailable" => ApplyOrder(query, x => x.MakeAvailable, descending),
                "hrsavail" => ApplyOrder(query, x => x.HrsAvail, descending),
                "spnumber" => ApplyOrder(query, x => x.SpNumber, descending),
                "name" or "staffname" => ApplyOrder(query, x => x.Name, descending),
                "workgroupgrade" or "wggrade" => ApplyOrder(query, x => x.WorkGroupGrade, descending),
                "personstatus" => ApplyOrder(query, x => x.PersonStatus, descending),
                "startdate" => ApplyOrder(query, x => x.StartDate, descending),
                "enddate" => ApplyOrder(query, x => x.EndDate, descending),
                "timerecorder" => ApplyOrder(query, x => x.TimeRecorder, descending),
                // Blank/NULL Class values carry no meaning, so they are ranked after every
                // populated value rather than sorted as an empty string (PostgreSQL places
                // '' before 'A' ascending and NULLs first descending, which pushed the
                // blanks to the top in both directions). The rank key is deliberately
                // always ascending so blanks stay at the bottom either way.
                "personclass" or "class" => descending
                    ? query.OrderBy(x => x.PersonClass == null || x.PersonClass.Trim() == "" ? 1 : 0)
                           .ThenByDescending(x => x.PersonClass)
                    : query.OrderBy(x => x.PersonClass == null || x.PersonClass.Trim() == "" ? 1 : 0)
                           .ThenBy(x => x.PersonClass),
                _ => query.OrderBy(x => x.Name)
            };
        }

        private static IQueryable<WorkGroupEmployeeView> ApplyOrder<T>(
            IQueryable<WorkGroupEmployeeView> query,
            Expression<Func<WorkGroupEmployeeView, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
