using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class DivisionGradeMaintenanceRepository : BaseRepository, IDivisionGradeMaintenanceRepository
    {
        public DivisionGradeMaintenanceRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<DivisionGradeMaintenance>> GetAllPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var q = _context.DivisionGrades.AsNoTracking().AsQueryable();

            q = ApplyFilter(q, query.Filter);
            q = ApplySorting(q, query.SortBy, query.Descending);

            var list = await q.ToListAsync();
            return ApplyPaging(list, query.Page, query.PageSize);
        }

        public async Task<DivisionGradeMaintenance?> GetByIdAsync(string divisionGradeCode)
        {
            if (string.IsNullOrWhiteSpace(divisionGradeCode))
            {
                return null;
            }

            return await _context.DivisionGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DivisionGradeCode == divisionGradeCode);
        }

        public async Task<DivisionGradeMaintenance> CreateAsync(DivisionGradeMaintenance divisionGrade)
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

        public async Task<DivisionGradeMaintenance> UpdateAsync(string originalCode, DivisionGradeMaintenance divisionGrade)
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

        private static IQueryable<DivisionGradeMaintenance> ApplyFilter(IQueryable<DivisionGradeMaintenance> query, string? filter)
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

        private static IQueryable<DivisionGradeMaintenance> ApplySorting(IQueryable<DivisionGradeMaintenance> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "divisiongradeCode" => descending ? query.OrderByDescending(x => x.DivisionGradeCode) : query.OrderBy(x => x.DivisionGradeCode),
                "gradecode" => descending ? query.OrderByDescending(x => x.GradeCode) : query.OrderBy(x => x.GradeCode),
                "division" => descending ? query.OrderByDescending(x => x.Division) : query.OrderBy(x => x.Division),
                "chargerate" => descending ? query.OrderByDescending(x => x.ChargeRate) : query.OrderBy(x => x.ChargeRate),
                "directrate" => descending ? query.OrderByDescending(x => x.DirectRate) : query.OrderBy(x => x.DirectRate),
                "payrate" => descending ? query.OrderByDescending(x => x.PayRate) : query.OrderBy(x => x.PayRate),
                "npr" => descending ? query.OrderByDescending(x => x.Npr) : query.OrderBy(x => x.Npr),
                "ohr" => descending ? query.OrderByDescending(x => x.Ohr) : query.OrderBy(x => x.Ohr),
                _ => query.OrderBy(x => x.DivisionGradeCode)
            };
        }
    }
}
