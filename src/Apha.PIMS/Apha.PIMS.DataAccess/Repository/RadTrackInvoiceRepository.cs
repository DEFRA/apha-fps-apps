// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: LINQ-first repository implementing IRadTrackInvoiceRepository; no stored procedures exist
 *     for this form — source uses direct table access via qryInvoices (SELECT query, not a SP).
 *   - GetAllAsync: applies four filter dimensions (Project, Contract, Year, Program) derived from
 *     qryInvoices.msaccsql WHERE clause and frmpimsinvoices.html toolbar dropdowns. Year filter maps
 *     to EXTRACT(year FROM duedate) equivalent using DueDate.Value.Year LINQ projection.
 *   - Program filter joins to ProjectRadTrackData (g_tlkpproject_radtrackdata) on project == parentproject
 *     to resolve Program — mirrors qryInvoices LEFT JOIN MY_tlkpProject ON tblRadTrackInvoice.Project.
 *   - GetTotalsAsync: sums PlannedAmount, DueAmount, ActualAmount using the same BuildFilterQuery helper
 *     to guarantee totals row matches the data grid.
 *   - ExistsAsync: AnyAsync duplicate-guard for InvoiceRef within Project+Contract scope; supports
 *     self-exclusion during Update via excludeInvoiceCounter.
 *   - All read operations use AsNoTracking.
 *   - DeleteAsync uses ExecuteDeleteAsync (set-based, no entity load required).
 *   - UpdateAsync uses _dbContext.Update + SaveChangesAsync (full entity replacement, matching Milestone
 *     pattern; caller loads entity first via GetByIdAsync).
 *   - CreateAsync uses _dbContext.Add + SaveChangesAsync; EF returns generated InvoiceCounter via identity.
 *   - Sorting defaults to InvoiceCounter descending (most recent first, consistent with grid prototype).
 *   - Private helpers: BuildFilterQuery, BuildProgramFilterQuery, ApplySorting, ApplyOrder.
 *
 * PRESERVED:
 *   - All filter dimensions from qryInvoices.msaccsql: project, contract, year (from duedate), program.
 *   - IRadTrackInvoiceRepository method signatures (GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync,
 *     DeleteAsync, GetTotalsAsync, ExistsAsync) — no deviations.
 *   - RadTrackInvoiceTotals aggregate (TotalPlannedAmount, TotalDueAmount, TotalActualAmount).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: qryInvoices.msaccsql references Access functions fnGetHiddenManager(),
 *     fnReportYear(), fnInReportMonthOrBefore(). These are excluded from the Year filter because the
 *     repository receives an explicit Year parameter from the UI toolbar rather than deriving it from
 *     an Access system function. Verify this is the correct behaviour for the ASP.NET Core app.
 *   - TRANSFORMENGINE TODO: qryInvoices OR-branch filters invoices where MY_tblContract.Year IS NULL
 *     OR MY_tblContract.Year = fnReportYear(). The repository currently filters on DueDate year
 *     instead, matching the UI Year dropdown. If contract-year filtering is needed, a join to
 *     RadTrackContract entity will be required (entity not yet registered in DbContext).
 *   - TRANSFORMENGINE TODO: Program filter joins to my_tlkpproject (Projects / MyTlkpProjects DbSet)
 *     via Parentproject == invoice.Project. my_tlkpproject has a composite PK (year, parentproject);
 *     the IN sub-select handles the multiple-rows-per-project correctly. Confirm this is the intended
 *     lookup table — qryInvoices references MY_tlkpProject which maps to my_tlkpproject.
 *   - TRANSFORMENGINE TODO: InvoicePaid is short — confirm short(0/1) is correct or evaluate bool migration.
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class RadTrackInvoiceRepository : BaseRepository, IRadTrackInvoiceRepository
    {
        private readonly PimsDbContext _dbContext;

        public RadTrackInvoiceRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: Paginated, filtered list — drives the invoice data grid in frmpimsinvoices.html.
        // Filter dimensions: Project, Contract, Year (from DueDate), Program (via join to g_tlkpproject_radtrackdata).
        // Year filter uses DueDate year rather than Access fnReportYear() — toolbar provides explicit year value.
        public async Task<PagedData<RadTrackInvoice>> GetAllAsync(PaginationParameters<RadTrackInvoiceFilter> parameters)
        {
            IQueryable<RadTrackInvoice> query = _dbContext.RadTrackInvoices.AsNoTracking();

            // TRANSFORMENGINE: Apply the four toolbar filter dimensions.
            query = BuildFilterQuery(query, parameters.Filter);

            // TRANSFORMENGINE: Program filter requires a join; handled separately to keep BuildFilterQuery pure.
            query = BuildProgramFilterQuery(query, parameters.Filter?.Program);

            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            List<RadTrackInvoice> all = await query.ToListAsync();
            return ApplyPaging(all, parameters.Page, parameters.PageSize);
        }

        // TRANSFORMENGINE: Single-record fetch by PK (InvoiceCounter) — used by Edit and Delete flows.
        public async Task<RadTrackInvoice?> GetByIdAsync(int invoiceCounter)
            => await _dbContext.RadTrackInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceCounter == invoiceCounter);

        // TRANSFORMENGINE: Insert a new invoice record; EF identity returns the generated InvoiceCounter.
        public async Task<RadTrackInvoice> CreateAsync(RadTrackInvoice entity)
        {
            _dbContext.RadTrackInvoices.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: Full update — caller is expected to load the entity first via GetByIdAsync,
        // apply changes, then pass the modified entity here. Matches Milestone repository pattern.
        public async Task<RadTrackInvoice> UpdateAsync(RadTrackInvoice entity)
        {
            _dbContext.RadTrackInvoices.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: Set-based delete via ExecuteDeleteAsync — no entity load needed.
        // Returns true if the row existed and was removed; false if not found.
        public async Task<bool> DeleteAsync(int invoiceCounter)
        {
            int rows = await _dbContext.RadTrackInvoices
                .Where(i => i.InvoiceCounter == invoiceCounter)
                .ExecuteDeleteAsync();
            return rows > 0;
        }

        // TRANSFORMENGINE: Aggregate totals — sums PlannedAmount, DueAmount, ActualAmount across the
        // same filtered set used in GetAllAsync, so the totals row always matches the grid.
        // Null amounts treated as 0 for summing (nullable double; Sum<double?> returns null for empty set).
        public async Task<RadTrackInvoiceTotals> GetTotalsAsync(RadTrackInvoiceFilter? filter)
        {
            IQueryable<RadTrackInvoice> query = _dbContext.RadTrackInvoices.AsNoTracking();

            query = BuildFilterQuery(query, filter);
            query = BuildProgramFilterQuery(query, filter?.Program);

            // TRANSFORMENGINE: CalculateTotals — sums the three amount columns over the filtered query.
            var totals = await query
                .GroupBy(_ => 1)
                .Select(g => new RadTrackInvoiceTotals
                {
                    TotalPlannedAmount = g.Sum(i => i.PlannedAmount ?? 0.0),
                    TotalDueAmount     = g.Sum(i => i.DueAmount ?? 0.0),
                    TotalActualAmount  = g.Sum(i => i.ActualAmount ?? 0.0)
                })
                .FirstOrDefaultAsync();

            // TRANSFORMENGINE: Return a zero-filled totals if the filtered set is empty.
            return totals ?? new RadTrackInvoiceTotals
            {
                TotalPlannedAmount = 0.0,
                TotalDueAmount     = 0.0,
                TotalActualAmount  = 0.0
            };
        }

        // TRANSFORMENGINE: AnyAsync duplicate guard — checks for existing InvoiceRef within the same
        // Project+Contract scope. excludeInvoiceCounter allows self-exclusion during Update so the
        // current record does not block its own save.
        public async Task<bool> ExistsAsync(
            string? project,
            string? contract,
            string? invoiceRef,
            int? excludeInvoiceCounter = null)
        {
            IQueryable<RadTrackInvoice> query = _dbContext.RadTrackInvoices.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(project))
                query = query.Where(i => i.Project == project);

            if (!string.IsNullOrWhiteSpace(contract))
                query = query.Where(i => i.Contract == contract);

            if (!string.IsNullOrWhiteSpace(invoiceRef))
                query = query.Where(i => i.InvoiceRef == invoiceRef);

            if (excludeInvoiceCounter.HasValue)
                query = query.Where(i => i.InvoiceCounter != excludeInvoiceCounter.Value);

            return await query.AnyAsync();
        }

        // ── Private helpers ──────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: BuildFilterQuery — applies Project, Contract, and Year filter predicates.
        // Program is handled separately in BuildProgramFilterQuery because it requires a join
        // to ProjectRadTrackData (g_tlkpproject_radtrackdata). All predicates are AND-combined.
        // Year maps to the calendar year extracted from DueDate — mirrors the toolbar Year dropdown.
        private static IQueryable<RadTrackInvoice> BuildFilterQuery(
            IQueryable<RadTrackInvoice> query,
            RadTrackInvoiceFilter? filter)
        {
            if (filter == null)
                return query;

            // TRANSFORMENGINE: Project filter — direct column equality on tblradtrackinvoice.project.
            if (!string.IsNullOrWhiteSpace(filter.Project))
                query = query.Where(i => i.Project == filter.Project);

            // TRANSFORMENGINE: Contract (Surveillance Contract) filter — column equality on contract.
            if (!string.IsNullOrWhiteSpace(filter.Contract))
                query = query.Where(i => i.Contract == filter.Contract);

            // TRANSFORMENGINE: Year filter — matches calendar year of DueDate.
            // Access qryInvoices used fnReportYear(); here the UI toolbar supplies an explicit year.
            // TRANSFORMENGINE TODO: Verify whether filter should apply to DateInvoiced or DueDate —
            // current implementation uses DueDate to match qryInvoices WHERE clause.
            if (filter.Year.HasValue)
                query = query.Where(i => i.DueDate != null && i.DueDate.Value.Year == filter.Year.Value);

            return query;
        }

        // TRANSFORMENGINE: BuildProgramFilterQuery — semi-join to my_tlkpproject (Projects entity /
        // MyTlkpProjects DbSet) to filter by program. Mirrors qryInvoices LEFT JOIN MY_tlkpProject
        // ON tblRadTrackInvoice.Project = MY_tlkpProject.ParentProject.
        // Projects has a composite PK (Year, Parentproject), so multiple rows may match per project;
        // the Contains sub-select deduplicates naturally via IN clause.
        private IQueryable<RadTrackInvoice> BuildProgramFilterQuery(
            IQueryable<RadTrackInvoice> query,
            string? program)
        {
            if (string.IsNullOrWhiteSpace(program))
                return query;

            // TRANSFORMENGINE: Semi-join: include invoices whose project exists in my_tlkpproject
            // with the requested program value. Uses MyTlkpProjects (Projects entity — program column).
            var matchingProjects = _dbContext.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Program == program)
                .Select(p => p.Parentproject);

            return query.Where(i => matchingProjects.Contains(i.Project));
        }

        // TRANSFORMENGINE: ApplySorting — maps SortBy string to typed expression; defaults to
        // InvoiceCounter descending (most recent record first, consistent with invoice grid behaviour).
        private static IQueryable<RadTrackInvoice> ApplySorting(
            IQueryable<RadTrackInvoice> query,
            string? sortBy,
            bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "project"            => ApplyOrder(query, i => i.Project,            descending),
                "contract"           => ApplyOrder(query, i => i.Contract,           descending),
                "plannedamount"      => ApplyOrder(query, i => i.PlannedAmount,      descending),
                "dueamount"          => ApplyOrder(query, i => i.DueAmount,          descending),
                "duedate"            => ApplyOrder(query, i => i.DueDate,            descending),
                "actualamount"       => ApplyOrder(query, i => i.ActualAmount,       descending),
                "dateinvoiced"       => ApplyOrder(query, i => i.DateInvoiced,       descending),
                "datejobsheetraised" => ApplyOrder(query, i => i.DateJobsheetRaised, descending),
                "invoiceref"         => ApplyOrder(query, i => i.InvoiceRef,         descending),
                "invoicepaid"        => ApplyOrder(query, i => i.InvoicePaid,        descending),
                _                    => ApplyOrder(query, i => i.InvoiceCounter,     descending: true)
            };
        }

        // TRANSFORMENGINE: ApplyOrder — generic ascending/descending helper matching Milestone pattern.
        private static IQueryable<RadTrackInvoice> ApplyOrder<T>(
            IQueryable<RadTrackInvoice> query,
            Expression<Func<RadTrackInvoice, T>> keySelector,
            bool descending)
            => descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
    }
}
