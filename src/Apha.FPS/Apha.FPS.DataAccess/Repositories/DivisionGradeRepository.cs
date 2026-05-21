using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class DivisionGradeRepository : BaseRepository, IDivisionGradeRepository
    {
        public DivisionGradeRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<DivisionGrade>> GetAllPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var q = _context.DivisionGrades.AsNoTracking().AsQueryable();

            q = ApplyFilter(q, query.Filter);
            q = ApplySorting(q, query.SortBy, query.Descending);

            var list = await q.ToListAsync();
            return ApplyPaging(list, query.Page, query.PageSize);
        }

        public async Task<DivisionGrade?> GetByIdAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
            {
                return null;
            }

            return await _context.DivisionGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DivisionGradeCode == divisionGradeCode);
        }

        public async Task<DivisionGrade> CreateAsync(DivisionGrade divisionGrade)
        {
            ArgumentNullException.ThrowIfNull(divisionGrade);

            divisionGrade.FpsYear = _context.FilterFpsYear;

            var existing = await _context.DivisionGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DivisionGradeCode == divisionGrade.DivisionGradeCode);

            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"A division grade with code '{divisionGrade.DivisionGradeCode}' already exists.");
            }

            _context.DivisionGrades.Add(divisionGrade);
            await _context.SaveChangesAsync();
            return divisionGrade;
        }

        public async Task<DivisionGrade> UpdateAsync(string originalCode, DivisionGrade divisionGrade)
        {
            ArgumentNullException.ThrowIfNull(divisionGrade);

            if (string.IsNullOrWhiteSpace(originalCode))
            {
                throw new ArgumentException("Original division grade code is required.", nameof(originalCode));
            }

            divisionGrade.FpsYear = _context.FilterFpsYear;

            if (!originalCode.Equals(divisionGrade.DivisionGradeCode, StringComparison.OrdinalIgnoreCase))
            {
                // PK is changing — delete and re-insert
                var existing = await _context.DivisionGrades
                    .FirstOrDefaultAsync(d => d.DivisionGradeCode == originalCode);

                if (existing == null)
                {
                    throw new InvalidOperationException($"Division grade '{originalCode}' not found.");
                }

                _context.DivisionGrades.Remove(existing);
                await _context.SaveChangesAsync();

                _context.DivisionGrades.Add(divisionGrade);
                await _context.SaveChangesAsync();

                return divisionGrade;
            }
            else
            {
                var existing = await _context.DivisionGrades
                    .FirstOrDefaultAsync(d => d.DivisionGradeCode == originalCode);

                if (existing == null)
                {
                    throw new InvalidOperationException($"Division grade '{originalCode}' not found.");
                }

                existing.GradeCode = divisionGrade.GradeCode;
                existing.Division = divisionGrade.Division;
                existing.ChargeRate = divisionGrade.ChargeRate;
                existing.DirectRate = divisionGrade.DirectRate;
                existing.PayRate = divisionGrade.PayRate;
                existing.Npr = divisionGrade.Npr;
                existing.Ohr = divisionGrade.Ohr;

                await _context.SaveChangesAsync();
                return existing;
            }
        }

        public async Task<bool> DeleteAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
            {
                return false;
            }

            var entity = await _context.DivisionGrades
                .FirstOrDefaultAsync(d => d.DivisionGradeCode == divisionGradeCode);

            if (entity == null)
            {
                return false;
            }

            _context.DivisionGrades.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetAllGradeCodesAsync()
        {
            return await _context.Grades
                .AsNoTracking()
                .Select(g => g.GradeCode)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
        }

        private static IQueryable<DivisionGrade> ApplyFilter(IQueryable<DivisionGrade> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return query;
            }

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                ?? new Dictionary<string, string>();

            if (filterDict.TryGetValue("DivisionGradeCode", out var divisionGradeCode) && divisionGradeCode != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.DivisionGradeCode, $"%{divisionGradeCode}%"));
            }

            if (filterDict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.GradeCode, $"%{gradeCode}%"));
            }

            if (filterDict.TryGetValue("Division", out var division) && division != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.Division, $"%{division}%"));
            }

            return query;
        }

        private static IQueryable<DivisionGrade> ApplySorting(IQueryable<DivisionGrade> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "divisiongradeCode" => Order(query, x => x.DivisionGradeCode, descending),
                "gradecode"         => Order(query, x => x.GradeCode, descending),
                "division"          => Order(query, x => x.Division, descending),
                "chargerate"        => Order(query, x => x.ChargeRate, descending),
                "directrate"        => Order(query, x => x.DirectRate, descending),
                "payrate"           => Order(query, x => x.PayRate, descending),
                "npr"               => Order(query, x => x.Npr, descending),
                "ohr"               => Order(query, x => x.Ohr, descending),
                _                   => query.OrderBy(x => x.DivisionGradeCode)
            };
        }

        private static IQueryable<DivisionGrade> Order<TKey>(
            IQueryable<DivisionGrade> query,
            System.Linq.Expressions.Expression<Func<DivisionGrade, TKey>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
