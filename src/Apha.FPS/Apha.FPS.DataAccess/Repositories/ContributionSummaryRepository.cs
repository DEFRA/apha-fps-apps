using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository for the Income/Contribution from Time Sales summary view (frmTimeSellerPC).
    /// </summary>
    public class ContributionSummaryRepository : IContributionSummaryRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ContributionSummaryRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        /// <inheritdoc/>
        public async Task<List<ContributionSummaryView>> GetBySellingPcAsync(string sellingPc, string? sortBy = null, bool descending = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sellingPc);

            var rows = await _dbContext.VQryFrmTimeSellerPcViews
                .AsNoTracking()
                .Where(x => x.SellingPc == sellingPc
                    && x.UserEmail != null
                    && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.WgGrade)
                .ToListAsync();

            return ApplySorting(rows, sortBy, descending);
        }

        /// <summary>
        /// Applies grid sorting. The percentage columns are derived (Hrs/AvHrs and AppHours/AvHrs);
        /// when Avail Hrs is 0 the form displays "!" and those rows are ordered as 0 so they stay together.
        /// </summary>
        private static List<ContributionSummaryView> ApplySorting(List<ContributionSummaryView> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows;

            Func<ContributionSummaryView, IComparable?>? keySelector = sortBy switch
            {
                "WorkGroup" => x => x.WorkGroup,
                "WgGrade" => x => x.WgGrade,
                "ProfitCentreGrade" => x => x.ProfitCentreGrade,
                "AvHrs" => x => x.AvHrs ?? 0d,
                "ChargeRate" => x => x.ChargeRate ?? 0m,
                "Hrs" => x => x.Hrs ?? 0d,
                "Fec" => x => x.Fec ?? 0m,
                "AppHours" => x => x.AppHours ?? 0d,
                "AppFec" => x => x.AppFec ?? 0m,
                "Ohr" => x => x.Ohr ?? 0m,
                "Contribution" => x => x.Contribution ?? 0m,
                "PctPlanned" or "PctPlannedDisplay" => x => GetPercentageSortKey(x.AvHrs, x.Hrs),
                "PctAssuredPlanned" or "PctAssuredPlannedDisplay" => x => GetPercentageSortKey(x.AvHrs, x.AppHours),
                _ => null
            };

            if (keySelector == null)
                return rows;

            return descending
                ? rows.OrderByDescending(keySelector).ToList()
                : rows.OrderBy(keySelector).ToList();
        }

        /// <summary>
        /// Sort key for a derived percentage column. Rows displayed as "!" (Avail Hrs is null or 0)
        /// are counted as 0 so they group together in the sequence.
        /// </summary>
        private static double GetPercentageSortKey(double? availableHours, double? hours)
        {
            if (!availableHours.HasValue || availableHours.Value == 0 || !hours.HasValue)
                return 0d;

            return hours.Value / availableHours.Value;
        }
    }
}
