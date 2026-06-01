using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProfitCentreGradeRepository : BaseRepository, IProfitCentreGradeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProfitCentreGradeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<PagedData<ProfitCentreGrade>> GetProfitCentreGradesAsync(
            PaginationParameters<string> query,
            string profitCentre)
        {
            var all = await _dbContext.ProfitCentreGradeViews
                .AsNoTracking()
                .Where(x => x.ProfitCentre == profitCentre
                         && x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Distinct()
                .OrderByDescending(x => x.ChargeRate)
                .Select(x => new ProfitCentreGrade
                {
                    PcGrade         = x.PcGrade        ?? string.Empty,
                    DivisionGrade   = x.DivisionGrade  ?? string.Empty,
                    GradeCode       = x.GradeCode      ?? string.Empty,
                    ProfitCentre    = x.ProfitCentre   ?? string.Empty,
                    ChargeRate      = x.ChargeRate,
                    DirectRate      = x.DirectRate,
                    PayRate         = x.PayRate,
                    NPR             = x.Npr,
                    OHR             = x.Ohr,
                    HrsAvailable    = x.HrsAvailable,
                    OldChargeRate   = x.OldChargeRate,
                    DefraChargeRate = x.DefraChargeRate,
                    FpsYear         = x.FpsYear.HasValue ? x.FpsYear.Value : 0
                })
                .ToListAsync();

            return ApplyPaging(all, query.Page, query.PageSize);
        }

        public async Task<PagedData<ProfitCentreGrade>> GetAllPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var q = _dbContext.ProfitCentreGrades.AsNoTracking().AsQueryable();
            q = ApplyFilter(q, query.Filter);
            q = (IQueryable<ProfitCentreGrade>)ApplySorting(q, query.SortBy, query.Descending);

            var list = await q.ToListAsync();
            return ApplyPaging(list, query.Page, query.PageSize);
        }

        public async Task<ProfitCentreGrade?> GetByIdAsync(string pcGrade)
        {
            return await _dbContext.ProfitCentreGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PcGrade == pcGrade);
        }

        public async Task<ProfitCentreGrade> CreateAsync(ProfitCentreGrade entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.FpsYear = _dbContext.FilterFpsYear;

            _dbContext.ProfitCentreGrades.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<ProfitCentreGrade> UpdateAsync(string originalPcGrade, ProfitCentreGrade entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (string.IsNullOrWhiteSpace(originalPcGrade))
                throw new ArgumentException("Original PcGrade is required.", nameof(originalPcGrade));

            entity.FpsYear = _dbContext.FilterFpsYear;

            if (!originalPcGrade.Equals(entity.PcGrade, StringComparison.OrdinalIgnoreCase))
            {
                // PK is changing — delete old row and insert new one
                var existing = await _dbContext.ProfitCentreGrades
                    .FirstOrDefaultAsync(x => x.PcGrade == originalPcGrade);

                _dbContext.ProfitCentreGrades.Remove(existing!);
                await _dbContext.SaveChangesAsync();

                _dbContext.ProfitCentreGrades.Add(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
            }
            else
            {
                var existing = await _dbContext.ProfitCentreGrades
                    .FirstOrDefaultAsync(x => x.PcGrade == originalPcGrade);

                existing!.DivisionGrade  = entity.DivisionGrade;
                existing.GradeCode      = entity.GradeCode;
                existing.ProfitCentre   = entity.ProfitCentre;
                existing.ChargeRate     = entity.ChargeRate;
                existing.DirectRate     = entity.DirectRate;
                existing.PayRate        = entity.PayRate;
                existing.NPR            = entity.NPR;
                existing.OHR            = entity.OHR;
                existing.HrsAvailable   = entity.HrsAvailable;

                await _dbContext.SaveChangesAsync();
                return existing;
            }
        }

        public async Task<bool> DeleteAsync(string pcGrade)
        {
            var entity = await _dbContext.ProfitCentreGrades
                .FirstOrDefaultAsync(x => x.PcGrade == pcGrade);

            if (entity == null)
                return false;

            _dbContext.ProfitCentreGrades.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates that ProfitCentre exists in tblkpprofitcentre.
        /// Converts tI_ProfitCentreGrade / tU_ProfitCentreGrade FK enforcement triggers.
        /// </summary>
        public async Task<bool> ProfitCentreExistsAsync(string profitCentre)
        {
            return await _dbContext.ProfitCentres
                .AsNoTracking()
                .AnyAsync(x => x.ProfitCentreId == profitCentre);
        }

        public async Task<List<string>> GetAllProfitCentreCodesAsync()
        {
            return await _dbContext.ProfitCentres
                .AsNoTracking()
                .Select(x => x.ProfitCentreId)
                .OrderBy(x => x)
                .ToListAsync();
        }

        private static IQueryable<ProfitCentreGrade> ApplyFilter(IQueryable<ProfitCentreGrade> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                ?? new Dictionary<string, string>();

            if (filterDict.TryGetValue("PcGrade", out var pcGrade) && pcGrade != null)
                query = query.Where(x => EF.Functions.ILike(x.PcGrade, $"%{pcGrade}%"));

            if (filterDict.TryGetValue("DivisionGrade", out var divGrade) && divGrade != null)
                query = query.Where(x => EF.Functions.ILike(x.DivisionGrade, $"%{divGrade}%"));

            if (filterDict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(x => EF.Functions.ILike(x.GradeCode, $"%{gradeCode}%"));

            if (filterDict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
                query = query.Where(x => EF.Functions.ILike(x.ProfitCentre, $"%{profitCentre}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProfitCentreGrade> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "pcgrade"       => Order(query, x => x.PcGrade, descending),
                "divisiongrade" => Order(query, x => x.DivisionGrade, descending),
                "gradecode"     => Order(query, x => x.GradeCode, descending),
                "profitcentre"  => Order(query, x => x.ProfitCentre, descending),
                "chargerate"    => Order(query, x => x.ChargeRate, descending),
                "directrate"    => Order(query, x => x.DirectRate, descending),
                "payrate"       => Order(query, x => x.PayRate, descending),
                "npr"           => Order(query, x => x.NPR, descending),
                "ohr"           => Order(query, x => x.OHR, descending),
                "hrsavailable"  => Order(query, x => x.HrsAvailable, descending),
                _               => query.OrderBy(x => x.PcGrade)
            };
        }

        private static IQueryable Order<TKey>(
            IQueryable<ProfitCentreGrade> query,
            System.Linq.Expressions.Expression<Func<ProfitCentreGrade, TKey>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
