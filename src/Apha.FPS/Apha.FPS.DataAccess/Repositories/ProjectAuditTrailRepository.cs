using System.Dynamic;
using System.Linq.Expressions;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectAuditTrailRepository : BaseRepository, IProjectAuditTrailRepository
    {
        private readonly FpsDbContext _dbContext;

        public ProjectAuditTrailRepository(FpsDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // Every audit grid runs the same pipeline over a different log table:
        // date-range window -> free-text search -> per-column grid filter -> sorting ->
        // materialise -> optional in-memory enrichment/filtering -> paging.
        // Only the table-specific parts are passed in, so the pipeline itself lives here once.
        private async Task<PagedData<T>> ExecuteAuditLogQueryAsync<T>(
            IQueryable<T> source,
            PaginationParameters<string> query,
            Expression<Func<T, DateTime?>> dateSelector,
            DateTime? fromDate,
            DateTime? toDate,
            Func<string, Expression<Func<T, bool>>> searchPredicate,
            Func<IQueryable<T>, string?, IQueryable<T>> columnFilter,
            Func<IQueryable<T>, string?, bool, IQueryable<T>> sorting,
            Func<List<T>, Task>? enrichAsync = null,
            Func<List<T>, string?, List<T>>? postFilter = null)
        {
            var q = ApplyDateRange(source, dateSelector, fromDate, toDate);

            if (!string.IsNullOrWhiteSpace(query.Search))
                q = q.Where(searchPredicate(query.Search.ToLower()));

            q = columnFilter(q, query.Filter);
            q = sorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();

            if (enrichAsync != null)
                await enrichAsync(result);

            if (postFilter != null)
                result = postFilter(result, query.Filter);

            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // fps.project_log has a direct parentproject column; no join required
        public Task<PagedData<ProjectLog>> GetProjectLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var source = _dbContext.ProjectLogs
                .AsNoTracking()
                .Where(p => p.ParentProject == parentProject);

            return ExecuteAuditLogQueryAsync(
                source, query, p => p.DateTime, fromDate, toDate,
                search => p =>
                    (p.InsertDelete != null && p.InsertDelete.ToLower().Contains(search)) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)),
                ApplyProjectLogFilter,
                ApplyProjectLogSorting);
        }

        // join to fps.tlkpjob (JobCodes) on jobcode to resolve the parentproject association
        public Task<PagedData<StaffJobLog>> GetStaffJobLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var source = from log in _dbContext.StaffJobLogs.AsNoTracking()
                         where log.JobCode == parentProject
                         select log;

            return ExecuteAuditLogQueryAsync(
                source, query, p => p.DateTime, fromDate, toDate,
                search => p =>
                    p.JobCode.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)),
                ApplyStaffJobLogFilter,
                ApplyStaffJobLogSorting,
                PopulateStaffNamesAsync,
                ApplyStaffJobLogNameFilter);
        }

        // fps.staffjob_log has no name column; resolve staff display names via a lookup
        // against vtblstaff_general (StaffGeneralViews), mirroring StaffJobRepository's
        // established staff-name enrichment pattern.
        private async Task PopulateStaffNamesAsync(List<StaffJobLog> logs)
        {
            var staffIds = logs.Select(l => l.StaffId).Distinct().ToList();
            if (staffIds.Count == 0)
                return;

            var staffNames = await _dbContext.StaffGeneralViews
                .AsNoTracking()
                .Where(s => s.StaffId != null && staffIds.Contains(s.StaffId))
                .ToListAsync();

            var staffNameMap = staffNames
                .GroupBy(s => s.StaffId!)
                .ToDictionary(g => g.Key, g => g.First().Name);

            foreach (var log in logs)
            {
                if (staffNameMap.TryGetValue(log.StaffId, out var name))
                    log.Name = name;
            }
        }

        // jobcode column in testreq_log is derived from projectbuyercode; join to JobCodes on jobcode
        public Task<PagedData<TestRequirementLog>> GetTestRequirementLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var source = from log in _dbContext.TestRequirementLogs.AsNoTracking()
                         where log.JobCode == parentProject
                         select log;

            return ExecuteAuditLogQueryAsync(
                source, query, p => p.DateTime, fromDate, toDate,
                search => p =>
                    (p.TestCode != null && p.TestCode.ToLower().Contains(search)) ||
                    (p.Buyer != null && p.Buyer.ToLower().Contains(search)) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)),
                ApplyTestRequirementLogFilter,
                ApplyTestRequirementLogSorting);
        }

        // join to JobCodes on jobcode to resolve the parentproject association
        public Task<PagedData<AnimalRequestLog>> GetAnimalRequestLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var source = from log in _dbContext.AnimalRequestLogs.AsNoTracking()
                         where log.JobCode == parentProject
                         select log;

            return ExecuteAuditLogQueryAsync(
                source, query, p => p.DateTime, fromDate, toDate,
                search => p =>
                    p.JobCode.ToLower().Contains(search) ||
                    p.AnimalType.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)),
                ApplyAnimalRequestLogFilter,
                ApplyAnimalRequestLogSorting);
        }

        // join to JobCodes on jobcode to resolve the parentproject association
        public Task<PagedData<AdditionalCostLog>> GetAdditionalCostLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var source = from log in _dbContext.AdditionalCostLogs.AsNoTracking()
                         where log.JobCode == parentProject
                         select log;

            return ExecuteAuditLogQueryAsync(
                source, query, p => p.DateTime, fromDate, toDate,
                search => p =>
                    p.JobCode.ToLower().Contains(search) ||
                    p.Account.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)),
                ApplyAdditionalCostLogFilter,
                ApplyAdditionalCostLogSorting,
                ResolveAdditionalCostLogUserEmailsAsync,
                ApplyAdditionalCostLogUserIdFilter);
        }

        // fps.additionalcosts_log.user_id was historically populated with a raw login/username
        // for some rows and, more recently, with the authenticated user's email address
        // (AdditionalCostRepository / ProjectRepository both write _requestContext.UserEmailId).
        // To guarantee the Exceptional Cost Changes grid's User_ID column always displays an
        // email address, resolve any legacy, non-email UserId values against fps.tblusers
        // (Username / Dt2Username -> UserEmail). This mirrors the StaffJobLog Name enrichment
        // pattern below (PopulateStaffNamesAsync) and only mutates the in-memory, no-tracking
        // result set — no changes are persisted back to the database.
        private async Task ResolveAdditionalCostLogUserEmailsAsync(List<AdditionalCostLog> logs)
        {
            var rawUserIds = logs
                .Where(l => !string.IsNullOrWhiteSpace(l.UserId) && !l.UserId!.Contains('@'))
                .Select(l => l.UserId!.Trim())
                .Distinct()
                .ToList();

            if (rawUserIds.Count == 0)
                return;

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(u => (u.Username != null && rawUserIds.Contains(u.Username))
                         || (u.Dt2Username != null && rawUserIds.Contains(u.Dt2Username)))
                .ToListAsync();

            var emailByUsername = users
                .Where(u => !string.IsNullOrWhiteSpace(u.Username) && !string.IsNullOrWhiteSpace(u.UserEmail))
                .GroupBy(u => u.Username!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().UserEmail, StringComparer.OrdinalIgnoreCase);

            var emailByDt2Username = users
                .Where(u => !string.IsNullOrWhiteSpace(u.Dt2Username) && !string.IsNullOrWhiteSpace(u.UserEmail))
                .GroupBy(u => u.Dt2Username!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().UserEmail, StringComparer.OrdinalIgnoreCase);

            foreach (var log in logs)
            {
                if (string.IsNullOrWhiteSpace(log.UserId) || log.UserId.Contains('@'))
                    continue;

                var key = log.UserId.Trim();

                if (emailByUsername.TryGetValue(key, out var email))
                    log.UserId = email;
                else if (emailByDt2Username.TryGetValue(key, out var email2))
                    log.UserId = email2;
                // else: no matching fps.tblusers record found — legacy value is left as-is.
            }
        }

        // ── Private column filter helpers ────────────────────────────────────────────────
        // The shared _DataGrid component posts its per-column filter row as a JSON object
        // keyed by the grid column PropertyName. Each audit grid deserializes that payload
        // and applies a case-insensitive "contains" match, mirroring ProjectRepository.

        // Every audit grid supports the same optional from/to date window over its
        // date_time column, so the range predicate is built once here.
        private static IQueryable<T> ApplyDateRange<T>(
            IQueryable<T> query,
            Expression<Func<T, DateTime?>> selector,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (fromDate.HasValue)
            {
                var predicate = Expression.Lambda<Func<T, bool>>(
                    Expression.GreaterThanOrEqual(
                        selector.Body,
                        Expression.Convert(Expression.Constant(fromDate.Value), typeof(DateTime?))),
                    selector.Parameters);
                query = query.Where(predicate);
            }

            if (toDate.HasValue)
            {
                var predicate = Expression.Lambda<Func<T, bool>>(
                    Expression.LessThanOrEqual(
                        selector.Body,
                        Expression.Convert(Expression.Constant(toDate.Value), typeof(DateTime?))),
                    selector.Parameters);
                query = query.Where(predicate);
            }

            return query;
        }

        private static IDictionary<string, object>? ParseFilterDictionary(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return null;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            return filterModel == null ? null : (IDictionary<string, object>)filterModel;
        }

        private static IQueryable<T> ApplyLike<T>(
            IQueryable<T> query,
            IDictionary<string, object> dict,
            string key,
            Expression<Func<T, string?>> selector)
        {
            if (!dict.TryGetValue(key, out var value) || value == null)
                return query;

            var text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return query;

            var term = text.ToLower();

            // ToLower().Contains() is used (rather than EF.Functions.ILike) so the predicate
            // remains provider agnostic and evaluates identically in unit tests.
            var toLower = Expression.Call(selector.Body, nameof(string.ToLower), Type.EmptyTypes);
            var contains = Expression.Call(
                toLower,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(term));
            var notNull = Expression.NotEqual(selector.Body, Expression.Constant(null, typeof(string)));
            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(notNull, contains), selector.Parameters);
            return query.Where(predicate);
        }

        private static IQueryable<T> ApplyTextFilters<T>(
            IQueryable<T> query,
            string? filter,
            (string Key, Expression<Func<T, string?>> Selector)[] textFilters)
        {
            var dict = ParseFilterDictionary(filter);
            if (dict == null)
                return query;

            foreach (var (key, selector) in textFilters)
                query = ApplyLike(query, dict, key, selector);

            return query;
        }

        // Per-grid column maps: grid PropertyName -> entity column. The matching logic
        // itself is shared by ApplyTextFilters, so each grid only declares its columns.
        private static readonly (string, Expression<Func<ProjectLog, string?>>)[] ProjectLogColumns =
        {
            ("ParentProject", x => x.ParentProject),
            ("ProjectTitle", x => x.ProjectTitle),
            ("Program", x => x.Program),
            ("Customer", x => x.Customer),
            ("Manager", x => x.Manager),
            ("ProjectStatus", x => x.ProjectStatus),
            ("CostBookNo", x => x.CostBookNo),
            ("Disease", x => x.Disease),
            ("Contract", x => x.Contract),
            ("ProjectParent", x => x.ProjectParent),
            ("ShortTitle", x => x.ShortTitle),
            ("OwningRc", x => x.OwningRc),
            ("UserId", x => x.UserId),
            ("InsertDelete", x => x.InsertDelete)
        };

        private static readonly (string, Expression<Func<StaffJobLog, string?>>)[] StaffJobLogColumns =
        {
            ("StaffId", x => x.StaffId),
            ("JobCode", x => x.JobCode),
            ("UserId", x => x.UserId),
            ("InsertDelete", x => x.InsertDelete)
        };

        private static readonly (string, Expression<Func<TestRequirementLog, string?>>)[] TestRequirementLogColumns =
        {
            ("TestCode", x => x.TestCode),
            ("Buyer", x => x.Buyer),
            ("ProjectBuyerCode", x => x.ProjectBuyerCode),
            ("TestBuyerCode", x => x.TestBuyerCode),
            ("UserId", x => x.UserId),
            ("InsertDelete", x => x.InsertDelete)
        };

        private static readonly (string, Expression<Func<AnimalRequestLog, string?>>)[] AnimalRequestLogColumns =
        {
            ("JobCode", x => x.JobCode),
            ("AnimalType", x => x.AnimalType),
            ("UserId", x => x.UserId),
            ("InsertDelete", x => x.InsertDelete)
        };

        private static readonly (string, Expression<Func<AdditionalCostLog, string?>>)[] AdditionalCostLogColumns =
        {
            ("JobCode", x => x.JobCode),
            ("Account", x => x.Account),
            ("Description", x => x.Description),
            ("Freq", x => x.Freq),
            ("Supplier", x => x.Supplier),
            ("InsertDelete", x => x.InsertDelete)
        };

        private static IQueryable<ProjectLog> ApplyProjectLogFilter(IQueryable<ProjectLog> query, string? filter)
            => ApplyTextFilters(query, filter, ProjectLogColumns);

        private static IQueryable<StaffJobLog> ApplyStaffJobLogFilter(IQueryable<StaffJobLog> query, string? filter)
            => ApplyTextFilters(query, filter, StaffJobLogColumns);

        private static IQueryable<TestRequirementLog> ApplyTestRequirementLogFilter(IQueryable<TestRequirementLog> query, string? filter)
            => ApplyTextFilters(query, filter, TestRequirementLogColumns);

        private static IQueryable<AnimalRequestLog> ApplyAnimalRequestLogFilter(IQueryable<AnimalRequestLog> query, string? filter)
            => ApplyTextFilters(query, filter, AnimalRequestLogColumns);

        private static IQueryable<AdditionalCostLog> ApplyAdditionalCostLogFilter(IQueryable<AdditionalCostLog> query, string? filter)
            => ApplyTextFilters(query, filter, AdditionalCostLogColumns);

        // Some grid columns are resolved in memory after the query runs, so their filters
        // cannot be translated to SQL and must be applied against the resolved value the
        // user actually sees in the grid.
        private static List<T> ApplyInMemoryFilter<T>(
            List<T> logs, string? filter, string key, Func<T, string?> selector)
        {
            var dict = ParseFilterDictionary(filter);
            if (dict == null || !dict.TryGetValue(key, out var value) || value == null)
                return logs;

            var text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return logs;

            return logs
                .Where(l => selector(l) is { } v && v.Contains(text, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Name is not a column on fps.staffjob_log; it is resolved in memory by
        // PopulateStaffNamesAsync, so its grid filter must also be applied in memory.
        private static List<StaffJobLog> ApplyStaffJobLogNameFilter(List<StaffJobLog> logs, string? filter)
        {
            return ApplyInMemoryFilter(logs, filter, "Name", l => l.Name);
        }

        // User_ID is rewritten to an email address after the query runs
        // (ResolveAdditionalCostLogUserEmailsAsync), so its grid filter is applied in memory
        // against the resolved value the user actually sees in the grid.
        private static List<AdditionalCostLog> ApplyAdditionalCostLogUserIdFilter(List<AdditionalCostLog> logs, string? filter)
        {
            return ApplyInMemoryFilter(logs, filter, "UserId", l => l.UserId);
        }

        // ── Private sorting helpers ──────────────────────────────────────────────────────
        // Each grid resolves its own sortBy -> column key selector, then defers the shared
        // asc/desc ordering to ApplySorting so the direction logic exists in one place only.

        private static IQueryable<T> ApplySorting<T>(
            IQueryable<T> q, bool descending, Expression<Func<T, object?>> keySelector)
        {
            return descending ? q.OrderByDescending(keySelector) : q.OrderBy(keySelector);
        }

        private static IQueryable<ProjectLog> ApplyProjectLogSorting(
            IQueryable<ProjectLog> q, string? sortBy, bool descending)
        {
            Expression<Func<ProjectLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "parentproject" => e => e.ParentProject,
                "projecttitle"  => e => e.ProjectTitle,
                "program"       => e => e.Program,
                "jobcode"       => e => e.JobCode,
                "date_time"     => e => e.DateTime,
                "insert_delete" => e => e.InsertDelete,
                "user_id"       => e => e.UserId,
                _               => e => e.DateTime,
            };
            return ApplySorting(q, descending, keySelector);
        }

        private static IQueryable<StaffJobLog> ApplyStaffJobLogSorting(
            IQueryable<StaffJobLog> q, string? sortBy, bool descending)
        {
            Expression<Func<StaffJobLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "staffid"       => e => e.StaffId,
                "jobcode"       => e => e.JobCode,
                "plannedhours"  => e => e.PlannedHours,
                "date_time"     => e => e.DateTime,
                "insert_delete" => e => e.InsertDelete,
                "user_id"       => e => e.UserId,
                _               => e => e.DateTime,
            };
            return ApplySorting(q, descending, keySelector);
        }

        private static IQueryable<TestRequirementLog> ApplyTestRequirementLogSorting(
            IQueryable<TestRequirementLog> q, string? sortBy, bool descending)
        {
            Expression<Func<TestRequirementLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "testcode"         => e => e.TestCode,
                "buyer"            => e => e.Buyer,
                "unitprice"        => e => e.UnitPrice,
                "norequired"       => e => e.NoRequired,
                "projectbuyercode" => e => e.ProjectBuyerCode,
                "testbuyercode"    => e => e.TestBuyerCode,
                "active"           => e => e.Active,
                "date_time"        => e => e.DateTime,
                "insert_delete"    => e => e.InsertDelete,
                "user_id"          => e => e.UserId,
                _                  => e => e.DateTime,
            };
            return ApplySorting(q, descending, keySelector);
        }

        private static IQueryable<AnimalRequestLog> ApplyAnimalRequestLogSorting(
            IQueryable<AnimalRequestLog> q, string? sortBy, bool descending)
        {
            Expression<Func<AnimalRequestLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "jobcode"         => e => e.JobCode,
                "animaltype"      => e => e.AnimalType,
                "numberofdays"    => e => e.NumberOfDays,
                "numberofanimals" => e => e.NumberOfAnimals,
                "date_time"       => e => e.DateTime,
                "insert_delete"   => e => e.InsertDelete,
                "user_id"         => e => e.UserId,
                _                 => e => e.DateTime,
            };
            return ApplySorting(q, descending, keySelector);
        }

        private static IQueryable<AdditionalCostLog> ApplyAdditionalCostLogSorting(
            IQueryable<AdditionalCostLog> q, string? sortBy, bool descending)
        {
            Expression<Func<AdditionalCostLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "jobcode"       => e => e.JobCode,
                "account"       => e => e.Account,
                "description"   => e => e.Description,
                "itemcost"      => e => e.ItemCost,
                "freq"          => e => e.Freq,
                "supplier"      => e => e.Supplier,
                "date_time"     => e => e.DateTime,
                "insert_delete" => e => e.InsertDelete,
                "user_id"       => e => e.UserId,
                _               => e => e.DateTime,
            };
            return ApplySorting(q, descending, keySelector);
        }
    }
}
