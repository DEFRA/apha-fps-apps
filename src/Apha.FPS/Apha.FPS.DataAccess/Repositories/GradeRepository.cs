using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// LINQ-first repository for CRUD and paged query operations on <see cref="Grade"/>.
    /// All queries respect the FpsYear HasQueryFilter applied by <see cref="FpsDbContext"/>.
    /// </summary>
    public class GradeRepository : BaseRepository, IGradeRepository
    {
        public GradeRepository(FpsDbContext context) : base(context)
        {
        }

        // TRANSFORMENGINE: GetAllPagedAsync — paged list with optional JSON filter and sort
        public async Task<PagedData<Grade>> GetAllPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var q = _context.Grades.AsNoTracking().AsQueryable();

            q = ApplyFilter(q, query.Filter);
            q = ApplySorting(q, query.SortBy, query.Descending);

            var list = await q.ToListAsync();
            return ApplyPaging(list, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetByIdAsync — single GradeCode lookup; FpsYear filter applied by DbContext HasQueryFilter
        public async Task<Grade?> GetByIdAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                return null;
            }

            return await _context.Grades
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GradeCode == gradeCode);
        }

        // TRANSFORMENGINE: CreateAsync — duplicate guard then Add + SaveChangesAsync; FpsYear from FilterFpsYear
        public async Task<Grade> CreateAsync(Grade grade)
        {
            ArgumentNullException.ThrowIfNull(grade);

            // TRANSFORMENGINE: inject the active FPS year so the partition key is always set correctly
            grade.FpsYear = _context.FilterFpsYear;

            var exists = await _context.Grades
                .AsNoTracking()
                .AnyAsync(g => g.GradeCode == grade.GradeCode);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"A grade with code '{grade.GradeCode}' already exists for FPS year {grade.FpsYear}.");
            }

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            return grade;
        }

        // TRANSFORMENGINE: UpdateAsync — GradeCode rename uses delete-and-reinsert; non-rename updates properties in place
        public async Task<Grade> UpdateAsync(string originalCode, Grade grade)
        {
            ArgumentNullException.ThrowIfNull(grade);

            if (string.IsNullOrWhiteSpace(originalCode))
            {
                throw new ArgumentException("Original grade code is required.", nameof(originalCode));
            }

            // TRANSFORMENGINE: inject active FPS year before persisting
            grade.FpsYear = _context.FilterFpsYear;

            if (!originalCode.Equals(grade.GradeCode, StringComparison.OrdinalIgnoreCase))
            {
                // TRANSFORMENGINE: PK is changing — delete old row and insert new row
                var existing = await _context.Grades
                    .FirstOrDefaultAsync(g => g.GradeCode == originalCode);

                if (existing == null)
                {
                    throw new InvalidOperationException($"Grade '{originalCode}' not found.");
                }

                _context.Grades.Remove(existing);
                await _context.SaveChangesAsync();

                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                return grade;
            }
            else
            {
                // TRANSFORMENGINE: same PK — update properties in place
                var existing = await _context.Grades
                    .FirstOrDefaultAsync(g => g.GradeCode == originalCode);

                if (existing == null)
                {
                    throw new InvalidOperationException($"Grade '{originalCode}' not found.");
                }

                existing.DescLong   = grade.DescLong;
                existing.AvSalary   = grade.AvSalary;
                existing.PactCode   = grade.PactCode;
                existing.AvLeaveHrs = grade.AvLeaveHrs;
                existing.AvSickHrs  = grade.AvSickHrs;

                await _context.SaveChangesAsync();
                return existing;
            }
        }

        // TRANSFORMENGINE: DeleteAsync — guard empty key, Remove + SaveChangesAsync, returns bool
        public async Task<bool> DeleteAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                return false;
            }

            var entity = await _context.Grades
                .FirstOrDefaultAsync(g => g.GradeCode == gradeCode);

            if (entity == null)
            {
                return false;
            }

            _context.Grades.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // TRANSFORMENGINE: ApplyFilter — JSON-keyed dictionary; supports GradeCode and Description (DescLong) filtering
        private static IQueryable<Grade> ApplyFilter(IQueryable<Grade> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return query;
            }

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                ?? new Dictionary<string, string>();

            if (filterDict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.GradeCode, $"%{gradeCode}%"));
            }

            if (filterDict.TryGetValue("Description", out var description) && description != null)
            {
                // TRANSFORMENGINE: "Description" filter key maps to DescLong column (desc_long in DDL)
                query = query.Where(x => x.DescLong != null && EF.Functions.ILike(x.DescLong, $"%{description}%"));
            }

            return query;
        }

        // TRANSFORMENGINE: ApplySorting — switch on lower-invariant sort key; default order by GradeCode
        private static IQueryable<Grade> ApplySorting(IQueryable<Grade> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "gradecode"   => Order(query, x => x.GradeCode,   descending),
                "description" => Order(query, x => x.DescLong,    descending),
                "avsalary"    => Order(query, x => x.AvSalary,    descending),
                _             => query.OrderBy(x => x.GradeCode)
            };
        }

        // TRANSFORMENGINE: Order<TKey> — generic ascending/descending helper (pattern from DivisionGradeRepository)
        private static IQueryable<Grade> Order<TKey>(
            IQueryable<Grade> query,
            System.Linq.Expressions.Expression<Func<Grade, TKey>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
