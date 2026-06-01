using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

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
                    PcGrade        = x.PcGrade        ?? string.Empty,
                    DivisionGrade  = x.DivisionGrade  ?? string.Empty,
                    GradeCode      = x.GradeCode      ?? string.Empty,
                    ProfitCentre   = x.ProfitCentre   ?? string.Empty,
                    ChargeRate     = x.ChargeRate,
                    DirectRate     = x.DirectRate,
                    PayRate        = x.PayRate,
                    NPR            = x.Npr,
                    OHR            = x.Ohr,
                    HrsAvailable   = x.HrsAvailable,
                    OldChargeRate  = x.OldChargeRate,
                    DefraChargeRate = x.DefraChargeRate,
                    FpsYear        = x.FpsYear.HasValue ? x.FpsYear.Value : 0
                })
                .ToListAsync();

            return ApplyPaging(all, query.Page, query.PageSize);
        }

        public async Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProfitCentreGrades
                .AsNoTracking()
                .Select(e => e.PcGrade)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }
    }
}
