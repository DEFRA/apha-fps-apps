using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectDepartmentIncomeRepository : BaseRepository, IProjectDepartmentIncomeRepository
    {
        private readonly IFpsRequestContext _requestContext;

        private static readonly string[] AnimalAcctCodes = { "LargeAnimals", "SmallAnimals", "Mice" };

        private const string AreaTime             = "Time";
        private const string AreaTests            = "Tests";
        private const string AreaAnimals          = "Animals";
        private const string AreaProjectSpecifics = "Project-specifics";

        public ProjectDepartmentIncomeRepository(FpsDbContext db, IFpsRequestContext requestContext) : base(db)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTimeIncomeAsync — mirrors qryDeptIncomeTime SELECT
        //
        // Access query reference (qryDeptIncomeTime):
        //   Source tables: tblWGEmployeeMAB, tlkpProject_MAP, CostCentre,
        //                  TimeCostCalcsMAP, WorkGroup_MAP
        //   Filter:  Class = Charge, Month between MonthFrom and MonthTo,
        //            ParentProject matches selected project
        //   Order:   ParentProject 
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTime>> GetTimeIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            var tcAggregated =
                from tc in _context.TimeCostCalcs.AsNoTracking()
                    .Where(t => t.FpsYear == fpsYear
                             && t.Class == "Charge"
                             && (int)t.Month >= monthFrom
                             && (int)t.Month <= monthTo)
                group tc by new { tc.WorkGroup, tc.Project, tc.Month, tc.StaffId, tc.FpsYear, tc.Name, tc.GradeCode, tc.ChargeRate } into g
                select new
                {
                    g.Key.WorkGroup,
                    g.Key.Project,
                    g.Key.Month,
                    g.Key.StaffId,
                    g.Key.Name,
                    g.Key.GradeCode,
                    g.Key.ChargeRate,
                    Pay     = g.Sum(x => x.Pay),
                    NonPay  = g.Sum(x => x.NonPay),
                    Overhead = g.Sum(x => x.Overhead),
                    Time    = g.Sum(x => x.Time),
                    Cost    = g.Sum(x => x.Cost),
                };

            var dbQuery =
                from tc in tcAggregated
                join wg in _context.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on tc.WorkGroup equals wg.WorkGroupName
                join proj in _context.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on tc.Project equals proj.ParentProject
                join emp in _context.WorkGroupEmployees.AsNoTracking()
                    .Where(e => e.FpsYear == fpsYear)
                    on tc.StaffId equals emp.PactId into empJoin
                from emp in empJoin.DefaultIfEmpty()
                join cc in _context.CostCentres.AsNoTracking()
                    .Where(c => c.FpsYear == fpsYear)
                    on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    TcMonth = tc.Month,
                    proj.IsDefraProject,
                    OCC = cc != null ? (double?)cc.CostCentreNo : null,
                    OPC = cc != null ? cc.ProfitCentre : null,
                    WgProfitCentre = wg.ProfitCentre,
                    WgCostCentre = wg.CostCentre,
                    tc.Name,
                    tc.GradeCode,
                    SpNumber = emp != null ? emp.SpNumber : null,
                    tc.ChargeRate,
                    tc.Pay,
                    tc.NonPay,
                    tc.Overhead,
                    tc.Time,
                    tc.Cost,
                };

            if (!string.IsNullOrEmpty(project))
                dbQuery = dbQuery.Where(r => r.ParentProject == project);

            var rows = await dbQuery.OrderBy(r => r.ParentProject).ToListAsync();

            return rows.Select(r => new DepartmentIncomeTime
            {
                Project = r.ParentProject,
                OracleProjectCode = r.OracleProjectCode,
                SubAccountCode = r.SubAccountCode,
                Month = (int)r.TcMonth,
                DefraProject = r.IsDefraProject != 0 ? "Yes" : "No",
                OCC = r.OCC.HasValue ? ((long)r.OCC.Value).ToString() : null,
                OPC = r.OPC,
                SPC = r.WgProfitCentre,
                SCC = r.WgCostCentre.HasValue ? ((long)r.WgCostCentre.Value).ToString() : null,
                Name = r.Name,
                GradeCode = r.GradeCode,
                SpNumber = r.SpNumber,
                ChargeRate = r.ChargeRate ?? 0m,
                Pay = r.Pay ?? 0m,
                NonPay = r.NonPay ?? 0m,
                Overhead = r.Overhead ?? 0m,
                Time = (decimal)(r.Time ?? 0.0),
                TotalCost = (decimal)(r.Cost ?? 0.0),
            }).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTestIncomeAsync — mirrors qryDeptIncomeTests SELECT
        //
        // Access SQL:
        //   FROM ((tlkpProject_MAP LEFT JOIN CostCentre ON tlkpProject_MAP.CostCentre = CostCentre.CostCentre)
        //     INNER JOIN (MonthlyOutput INNER JOIN WorkGroup_MAP ON MonthlyOutput.WorkGroup = WorkGroup_MAP.WorkGroup)
        //     ON tlkpProject_MAP.ParentProject = MonthlyOutput.Buyer)
        //   INNER JOIN tblTestRequ_TM ON MonthlyOutput.Buyer = tblTestRequ_TM.JobCode
        //                            AND MonthlyOutput.TestCode = tblTestRequ_TM.TestCode
        //   WHERE ParentProject Like nz(fnDeptIncomeProject(),"*")
        //     AND Month BETWEEN fnDeptIncomeMonthFrom() AND fnDeptIncomeMonthTo()
        //   ORDER BY ParentProject
        //
        // Note: tblTestRequ_TM in Access is fps.tlkptestreqmt (TestRequirement entity):
        //   Buyer=JobCode, TestCode=TestCode, UnitPrice=TestPrice, active=1 (active only)
        // Access MonthlyOutput saved query groups volume by (buyer,testcode,month,workgroup) before
        // joining WorkGroup_MAP — replicated here with moAggregated group-by step.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTest>> GetTestIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            // Two-step: DB query returns raw anonymous type; in-memory projection formats strings.
            //
            // Access qryDeptIncomeTest shows one row per (buyer, testcode, month, workgroup).
            // Month IS part of the group key — each period month produces its own row.
            // Any row-count difference vs Access reflects data differences between databases.
            var moAggregated =
                from mo in _context.MonthlyOutputs.AsNoTracking()
                    .Where(m => m.FpsYear == fpsYear
                             && (int)m.Month >= monthFrom
                             && (int)m.Month <= monthTo)
                group mo by new { mo.Buyer, mo.TestCode, mo.Month, mo.WorkGroup } into g
                select new
                {
                    g.Key.Buyer,
                    g.Key.TestCode,
                    g.Key.Month,
                    g.Key.WorkGroup,
                    Volume = g.Sum(x => x.Volume),
                };

            var dbQuery =
                from mo in moAggregated
                join wg in _context.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on mo.WorkGroup equals wg.WorkGroupName
                join proj in _context.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on mo.Buyer equals proj.ParentProject
                join tr in _context.TestRequirements.AsNoTracking()
                    .Where(t => t.FpsYear == fpsYear && t.Active == 1)
                    on new { mo.Buyer, mo.TestCode } equals new { tr.Buyer, tr.TestCode }
                join cc in _context.CostCentres.AsNoTracking()
                    .Where(c => c.FpsYear == fpsYear)
                    on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    proj.IsDefraProject,
                    OCC = cc != null ? (double?)cc.CostCentreNo : null,
                    OPC = cc != null ? cc.ProfitCentre : null,
                    MoMonth = mo.Month,
                    WgProfitCentre = wg.ProfitCentre,
                    mo.WorkGroup,
                    WgCostCentre = wg.CostCentre,
                    mo.TestCode,
                    mo.Volume,
                    tr.UnitPrice,
                };

            if (!string.IsNullOrEmpty(project))
                dbQuery = dbQuery.Where(r => r.ParentProject == project);

            var rows = await dbQuery.OrderBy(r => r.ParentProject).ToListAsync();

            return rows.Select(r =>
            {
                var unitPrice = r.UnitPrice ?? 0m;
                var volume = (decimal)(r.Volume ?? 0.0);
                return new DepartmentIncomeTest
                {
                    Project = r.ParentProject,
                    OracleProjectCode = r.OracleProjectCode,
                    SubAccountCode = r.SubAccountCode,
                    DefraProject = r.IsDefraProject != 0 ? "Yes" : "No",
                    OPC = r.OPC,
                    OCC = r.OCC.HasValue ? ((long)r.OCC.Value).ToString() : null,
                    Month = (int)r.MoMonth,
                    SPC = r.WgProfitCentre,
                    WorkGroup = r.WorkGroup,
                    SCC = r.WgCostCentre.HasValue ? ((long)r.WgCostCentre.Value).ToString() : null,
                    TestCode = r.TestCode,
                    Volume = volume,
                    TestPrice = unitPrice,
                    TotalCost = unitPrice * volume,
                };
            }).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTestSnapshotIncomeAsync — mirrors SQL Server fPeriodTests TVF
        //
        // SQL Server logic:
        //   SELECT ..., Sum(Volume), Sum(TestPrice), Sum(TotalCost)
        //   FROM (
        //     SELECT ... FROM Period_MonthlyOutput WHERE period = @endPeriod
        //     UNION ALL
        //     SELECT ..., -Volume, -TestPrice, TotalCost FROM Period_MonthlyOutput WHERE period = @startPeriod
        //   ) sq
        //   GROUP BY Project, ..., Month, SPC, SCC, WorkGroup, TestCode
        //   HAVING abs(sum(Volume)) > 0
        //     AND Project = ISNULL(@project, Project)
        //
        // period_monthlyoutput has no fpsyear column.  FpsYear is obtained by joining
        // period_monthlyoutput.period → tblperiod.endperiod → tblperiod.fpsyear.
        // The delta (end - start) shows the net change between two period snapshots.
        // Rows where volume cancelled out (abs(sum) == 0) are excluded by the HAVING clause.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTest>> GetTestSnapshotIncomeAsync(
            string? project, int startPeriod, int endPeriod)
        {
            var fpsYear = _requestContext.FpsYear;

            // Collect the period numbers (endperiod values) that belong to the current FpsYear.
            // period_monthlyoutput.period stores the integer value of tblperiod.endperiod.
            var validPeriodNumbers = (await _context.Periods
                .AsNoTracking()
                .Where(p => p.FpsYear == fpsYear)
                .ToListAsync())
                .Select(p => (int)p.EndPeriod)
                .ToList();

            // Fetch end-period and start-period rows, scoped to the current FpsYear via the join.
            var endRows = await _context.PeriodMonthlyOutputs
                .AsNoTracking()
                .Where(r => r.Period == endPeriod
                         && validPeriodNumbers.Contains(r.Period)
                         && (project == null || r.Project == project))
                .ToListAsync();

            var startRows = await _context.PeriodMonthlyOutputs
                .AsNoTracking()
                .Where(r => r.Period == startPeriod
                         && validPeriodNumbers.Contains(r.Period)
                         && (project == null || r.Project == project))
                .ToListAsync();

            // UNION ALL: end rows positive, start rows negated
            var unionAll =
                endRows.Select(r => new
                {
                    r.Project,
                    r.OracleProjectCode,
                    r.SubAccountCode,
                    r.IsDefraProject,
                    r.Opc,
                    r.Occ,
                    r.Month,
                    r.Spc,
                    r.Scc,
                    r.WorkGroup,
                    r.TestCode,
                    Volume    = r.Volume ?? 0.0,
                    TestPrice = r.TestPrice ?? 0m,
                    TotalCost = r.TotalCost ?? 0m,
                })
                .Concat(startRows.Select(r => new
                {
                    r.Project,
                    r.OracleProjectCode,
                    r.SubAccountCode,
                    r.IsDefraProject,
                    r.Opc,
                    r.Occ,
                    r.Month,
                    r.Spc,
                    r.Scc,
                    r.WorkGroup,
                    r.TestCode,
                    Volume    = -(r.Volume ?? 0.0),
                    TestPrice = -(r.TestPrice ?? 0m),
                    TotalCost = r.TotalCost ?? 0m,
                }));

            // GROUP BY then HAVING abs(sum(volume)) > 0
            var grouped = unionAll
                .GroupBy(r => new
                {
                    r.Project,
                    r.OracleProjectCode,
                    r.SubAccountCode,
                    r.IsDefraProject,
                    r.Opc,
                    r.Occ,
                    r.Month,
                    r.Spc,
                    r.Scc,
                    r.WorkGroup,
                    r.TestCode,
                })
                .Select(g => new
                {
                    g.Key.Project,
                    g.Key.OracleProjectCode,
                    g.Key.SubAccountCode,
                    g.Key.IsDefraProject,
                    g.Key.Opc,
                    g.Key.Occ,
                    g.Key.Month,
                    g.Key.Spc,
                    g.Key.Scc,
                    g.Key.WorkGroup,
                    g.Key.TestCode,
                    Volume    = g.Sum(r => r.Volume),
                    TestPrice = g.Sum(r => r.TestPrice),
                    TotalCost = g.Sum(r => r.TotalCost),
                })
                .Where(r => Math.Abs(r.Volume) > 0)
                .OrderBy(r => r.Project)
                .ToList();

            return grouped.Select(r => new DepartmentIncomeTest
            {
                Project           = r.Project,
                OracleProjectCode = r.OracleProjectCode,
                SubAccountCode    = r.SubAccountCode,
                DefraProject      = r.IsDefraProject,
                OPC               = r.Opc,
                OCC               = r.Occ.HasValue ? ((long)r.Occ.Value).ToString() : null,
                Month             = (int)r.Month,
                SPC               = r.Spc,
                WorkGroup         = r.WorkGroup,
                SCC               = r.Scc.HasValue ? ((long)r.Scc.Value).ToString() : null,
                TestCode          = r.TestCode,
                Volume            = (decimal)r.Volume,
                TestPrice         = r.TestPrice,
                TotalCost         = r.TotalCost,
            }).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetAnimalIncomeAsync — mirrors qryDeptIncomeAnimals SELECT
        //
        // Access SQL:
        //   FROM (CostCentre RIGHT JOIN tlkpProject_MAP ON CostCentre.CostCentre = tlkpProject_MAP.CostCentre)
        //     INNER JOIN Proj_SubContract ON tlkpProject_MAP.ParentProject = Proj_SubContract.Project
        //   WHERE AcctCode IN ("LargeAnimals","SmallAnimals","Mice")
        //     AND ParentProject Like nz(fnDeptIncomeProject(),"*")
        //     AND Month BETWEEN fnDeptIncomeMonthFrom() AND fnDeptIncomeMonthTo()
        //   ORDER BY ParentProject
        //
        // VBA helpers ported to C# private static methods:
        //   fnAnimalDesc(d) → ParseAnimalDesc(d)
        //   fnAnimalDays(d) → ParseAnimalDays(d)
        //   DLookUp("[DailyRate]","tblAnimals","[AnimalType]=...") → Animals join in LINQ
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeAnimal>> GetAnimalIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            // Mirrors qryDeptIncomeAnimals (Access).
            // SPC='SSSD' and SCC=35227 are intentionally hardcoded constants — both qryDeptIncomeAnimals
            // (Access view) and fPeriodAnimals (SQL Server function) use these fixed values for all
            // animal housing rows.
            var fpsYear = _requestContext.FpsYear;

            var subContracts = await
                (from sc in _context.ProjectSubContracts.AsNoTracking()
                     .Where(s => s.FpsYear == fpsYear
                              && s.AcctCode != null
                              && AnimalAcctCodes.Contains(s.AcctCode)
                              && s.Month.HasValue
                              && (int)s.Month.Value >= monthFrom
                              && (int)s.Month.Value <= monthTo)
                 join proj in _context.Projects.AsNoTracking()
                     .Where(p => p.FpsYear == fpsYear)
                     on sc.Project equals proj.ParentProject
                 join cc in _context.CostCentres.AsNoTracking()
                     .Where(c => c.FpsYear == fpsYear)
                     on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                 from cc in ccJoin.DefaultIfEmpty()
                 select new
                 {
                     proj.ParentProject,
                     proj.OracleProjectCode,
                     proj.SubAccountCode,
                     proj.IsDefraProject,
                     OCC   = cc != null ? (double?)cc.CostCentreNo : null,
                     OPC   = cc != null ? cc.ProfitCentre          : (string?)null,
                     Month = (int)(sc.Month ?? 0),
                     Amount = sc.Amount ?? 0m,
                 })
                .ToListAsync();

            if (!string.IsNullOrEmpty(project))
                subContracts = subContracts.Where(r => r.ParentProject == project).ToList();

            // GROUP BY all key fields, SUM(Amount) AS TotalCost.
            // HAVING abs(Sum(amount)) > 0.001 — suppress zero-net rows.
            var result = subContracts
                .GroupBy(r => new
                {
                    r.ParentProject,
                    r.OracleProjectCode,
                    r.SubAccountCode,
                    r.IsDefraProject,
                    r.OCC,
                    r.OPC,
                    r.Month,
                })
                .Select(g => new { Key = g.Key, TotalCost = g.Sum(r => r.Amount) })
                .Where(g => Math.Abs(g.TotalCost) > 0.001m)
                .Select(g => new DepartmentIncomeAnimal
                {
                    Project           = g.Key.ParentProject,
                    OracleProjectCode = g.Key.OracleProjectCode,
                    SubAccountCode    = g.Key.SubAccountCode,
                    DefraProject      = g.Key.IsDefraProject != 0 ? "Yes" : "No",
                    OPC               = g.Key.OPC,
                    OCC               = g.Key.OCC.HasValue ? ((long)g.Key.OCC.Value).ToString() : null,
                    Month             = g.Key.Month,
                    SPC               = "SSSD",
                    SCC               = "35227",
                    TotalCost         = g.TotalCost,
                })
                .OrderBy(r => r.Project)
                .ToList();

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetAdditionalIncomeAsync — mirrors fPeriodDeptIncomeExceptional (Snapshot Data tab)
        //
        // The Access snapshot path uses the fPeriod* SQL Server TVF, which includes
        // HAVING Sum(Amount) > 0 — rows with a negative net TotalCost are excluded.
        // This differs from qryDeptIncomeExceptional (Current Data Old Style) which shows
        // all rows, including negative ones.
        //
        // Access SQL (snapshot variant / fPeriod*):
        //   FROM (CostCentre RIGHT JOIN tlkpProject_MAP ON CostCentre.CostCentre = tlkpProject_MAP.CostCentre)
        //     INNER JOIN Proj_SubContract ON tlkpProject_MAP.ParentProject = Proj_SubContract.Project
        //   WHERE AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice")
        //   GROUP BY ParentProject, OracleProjectCode, SubAccountCode, IIf(...), OPC, OCC, Month
        //   HAVING ParentProject Like nz(fnDeptIncomeProject(),"*")
        //     AND Month BETWEEN fnDeptIncomeMonthFrom() AND fnDeptIncomeMonthTo()
        //     AND Sum(Amount) > 0        ← excludes negative-net rows in Snapshot tab
        //   ORDER BY ParentProject
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            // Mirrors fPeriodDeptIncomeExceptional snapshot behaviour (Access).
            var fpsYear = _requestContext.FpsYear;

            var query =
                from sc in _context.ProjectSubContracts.AsNoTracking()
                    .Where(s => s.FpsYear == fpsYear
                             && s.AcctCode != null
                             && !AnimalAcctCodes.Contains(s.AcctCode)
                             && s.Month.HasValue
                             && (int)s.Month.Value >= monthFrom
                             && (int)s.Month.Value <= monthTo)
                join proj in _context.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on sc.Project equals proj.ParentProject
                join cc in _context.CostCentres.AsNoTracking()
                    .Where(c => c.FpsYear == fpsYear)
                    on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    proj.IsDefraProject,
                    OCC = cc != null ? (double?)cc.CostCentreNo : null,
                    OPC = cc != null ? cc.ProfitCentre : (string?)null,
                    Month = (int)(sc.Month ?? 0),
                    Amount = sc.Amount ?? 0m,
                };

            if (!string.IsNullOrEmpty(project))
                query = query.Where(r => r.ParentProject == project);

            var grouped = await query.ToListAsync();

            var result = grouped
                .GroupBy(r => new
                {
                    r.ParentProject,
                    r.OracleProjectCode,
                    r.SubAccountCode,
                    r.IsDefraProject,
                    r.OCC,
                    r.OPC,
                    r.Month,
                })
                .Select(g => new { Key = g.Key, TotalCost = g.Sum(r => r.Amount) })
                .Where(g => g.TotalCost > 0)          // HAVING Sum(Amount) > 0 — snapshot only
                .Select(g => new DepartmentIncomeAdditional
                {
                    Project           = g.Key.ParentProject,
                    OracleProjectCode = g.Key.OracleProjectCode,
                    SubAccountCode    = g.Key.SubAccountCode,
                    DefraProject      = g.Key.IsDefraProject != 0 ? "Yes" : "No",
                    OPC               = g.Key.OPC,
                    OCC               = g.Key.OCC.HasValue ? ((long)g.Key.OCC.Value).ToString() : null,
                    Month             = g.Key.Month,
                    TotalCost         = g.TotalCost,
                })
                .OrderBy(r => r.Project)
                .ToList();

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTotalsAsync — mirrors qryDeptIncomeTotals TRANSFORM/PIVOT
        //
        // Access SQL:
        //   TRANSFORM Sum(qryDeptIncomeTotals_sub.TotalCost) AS SumOfTotalCost
        //   SELECT Project, OracleProjectCode, Sum(TotalCost) AS TotalCosts
        //   FROM qryDeptIncomeTotals_sub
        //   GROUP BY Project, OracleProjectCode
        //   PIVOT Area IN ("Time","Tests","Animals","Project-specifics")
        //
        // qryDeptIncomeTotals_sub is a UNION ALL of qryDeptIncomeTime + qryDeptIncomeTests
        //   + qryDeptIncomeAnimals + qryDeptIncomeExceptional
        //
        // LINQ PIVOT emulation: GroupBy Project+OracleProjectCode, then conditional Sum per area.
        // To avoid N+1 overhead, the four sub-queries are executed and unioned in memory.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTotals>> GetTotalsAsync(
            string? project, int monthFrom, int monthTo)
        {
            var timeRows = await GetTimeIncomeAsync(project, monthFrom, monthTo);
            var testRows = await GetTestIncomeAsync(project, monthFrom, monthTo);
            var animalRows = await GetAnimalIncomeAsync(project, monthFrom, monthTo);
            var additionalRows = await GetAdditionalIncomeAsync(project, monthFrom, monthTo);

            var unionAll = timeRows
                .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaTime })
                .Concat(testRows
                    .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaTests }))
                .Concat(animalRows
                    .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaAnimals }))
                .Concat(additionalRows
                    .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaProjectSpecifics }))
                .ToList();

            var result = unionAll
                .GroupBy(r => new { r.Project, r.OracleProjectCode })
                .Select(g =>
                {
                    var timeCost = g.Where(r => r.Area == AreaTime).Sum(r => (decimal?)r.TotalCost);
                    var testsCost = g.Where(r => r.Area == AreaTests).Sum(r => (decimal?)r.TotalCost);
                    var animalsCost = g.Where(r => r.Area == AreaAnimals).Sum(r => (decimal?)r.TotalCost);
                    var projSpecCost = g.Where(r => r.Area == AreaProjectSpecifics).Sum(r => (decimal?)r.TotalCost);

                    return new DepartmentIncomeTotals
                    {
                        Project = g.Key.Project,
                        OracleProjectCode = g.Key.OracleProjectCode,
                        TotalCosts = g.Sum(r => r.TotalCost),
                        TimeCost = timeCost is null or 0m ? null : timeCost,
                        TestsCost = testsCost is null or 0m ? null : testsCost,
                        AnimalsCost = animalsCost is null or 0m ? null : animalsCost,
                        ProjectSpecificsCost = projSpecCost is null or 0m ? null : projSpecCost,
                    };
                })
                .OrderBy(r => r.Project)
                .ToList();

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTimeIncomeCurrentAsync — mirrors qryDeptIncomeTime (Current Data Old Style)
        //
        // Unlike GetTimeIncomeAsync (snapshot), this does NOT group by JobCode.
        // qryDeptIncomeTime uses raw TimeCostCalcs rows, producing one row per
        // (WorkGroup, Project, Month, StaffId, JobCode) combination.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTime>> GetTimeIncomeCurrentAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            var dbQuery =
                from tc in _context.TimeCostCalcs.AsNoTracking()
                    .Where(t => t.FpsYear == fpsYear
                             && t.Class == "Charge"
                             && (int)t.Month >= monthFrom
                             && (int)t.Month <= monthTo)
                join wg in _context.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on tc.WorkGroup equals wg.WorkGroupName
                join proj in _context.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on tc.Project equals proj.ParentProject
                join emp in _context.WorkGroupEmployees.AsNoTracking()
                    .Where(e => e.FpsYear == fpsYear)
                    on tc.StaffId equals emp.PactId into empJoin
                from emp in empJoin.DefaultIfEmpty()
                join cc in _context.CostCentres.AsNoTracking()
                    .Where(c => c.FpsYear == fpsYear)
                    on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    TcMonth = tc.Month,
                    proj.IsDefraProject,
                    OCC = cc != null ? (double?)cc.CostCentreNo : null,
                    OPC = cc != null ? cc.ProfitCentre : null,
                    WgProfitCentre = wg.ProfitCentre,
                    WgCostCentre = wg.CostCentre,
                    tc.Name,
                    tc.GradeCode,
                    SpNumber = emp != null ? emp.SpNumber : null,
                    tc.ChargeRate,
                    tc.Pay,
                    tc.NonPay,
                    tc.Overhead,
                    tc.Time,
                    tc.Cost,
                };

            if (!string.IsNullOrEmpty(project))
                dbQuery = dbQuery.Where(r => r.ParentProject == project);

            var rows = await dbQuery.OrderBy(r => r.ParentProject).ToListAsync();

            return rows.Select(r => new DepartmentIncomeTime
            {
                Project = r.ParentProject,
                OracleProjectCode = r.OracleProjectCode,
                SubAccountCode = r.SubAccountCode,
                Month = (int)r.TcMonth,
                DefraProject = r.IsDefraProject != 0 ? "Yes" : "No",
                OCC = r.OCC.HasValue ? ((long)r.OCC.Value).ToString() : null,
                OPC = r.OPC,
                SPC = r.WgProfitCentre,
                SCC = r.WgCostCentre.HasValue ? ((long)r.WgCostCentre.Value).ToString() : null,
                Name = r.Name,
                GradeCode = r.GradeCode,
                SpNumber = r.SpNumber,
                ChargeRate = r.ChargeRate ?? 0m,
                Pay = r.Pay ?? 0m,
                NonPay = r.NonPay ?? 0m,
                Overhead = r.Overhead ?? 0m,
                Time = (decimal)(r.Time ?? 0.0),
                TotalCost = (decimal)(r.Cost ?? 0.0),
            }).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTestIncomeCurrentAsync — mirrors qryDeptIncomeTests (Current Data Old Style)
        //
        // Unlike GetTestIncomeAsync (snapshot), this does NOT aggregate volume by buyer+testcode+month.
        // qryDeptIncomeTests uses raw MonthlyOutput rows joined to WorkGroup and vtblTestRequ_TM.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTest>> GetTestIncomeCurrentAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            var dbQuery =
                from mo in _context.MonthlyOutputs.AsNoTracking()
                    .Where(m => m.FpsYear == fpsYear
                             && (int)m.Month >= monthFrom
                             && (int)m.Month <= monthTo)
                join wg in _context.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on mo.WorkGroup equals wg.WorkGroupName
                join proj in _context.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on mo.Buyer equals proj.ParentProject
                join tr in _context.TestRequirements.AsNoTracking()
                    .Where(t => t.FpsYear == fpsYear && t.Active == 1)
                    on new { mo.Buyer, mo.TestCode } equals new { tr.Buyer, tr.TestCode }
                join cc in _context.CostCentres.AsNoTracking()
                    .Where(c => c.FpsYear == fpsYear)
                    on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    proj.IsDefraProject,
                    OCC = cc != null ? (double?)cc.CostCentreNo : null,
                    OPC = cc != null ? cc.ProfitCentre : null,
                    MoMonth = mo.Month,
                    WgProfitCentre = wg.ProfitCentre,
                    mo.WorkGroup,
                    WgCostCentre = wg.CostCentre,
                    mo.TestCode,
                    mo.Volume,
                    tr.UnitPrice,
                };

            if (!string.IsNullOrEmpty(project))
                dbQuery = dbQuery.Where(r => r.ParentProject == project);

            var rows = await dbQuery.OrderBy(r => r.ParentProject).ToListAsync();

            return rows.Select(r =>
            {
                var unitPrice = r.UnitPrice ?? 0m;
                var volume = (decimal)(r.Volume ?? 0.0);
                return new DepartmentIncomeTest
                {
                    Project = r.ParentProject,
                    OracleProjectCode = r.OracleProjectCode,
                    SubAccountCode = r.SubAccountCode,
                    DefraProject = r.IsDefraProject != 0 ? "Yes" : "No",
                    OPC = r.OPC,
                    OCC = r.OCC.HasValue ? ((long)r.OCC.Value).ToString() : null,
                    Month = (int)r.MoMonth,
                    SPC = r.WgProfitCentre,
                    WorkGroup = r.WorkGroup,
                    SCC = r.WgCostCentre.HasValue ? ((long)r.WgCostCentre.Value).ToString() : null,
                    TestCode = r.TestCode,
                    Volume = volume,
                    TestPrice = unitPrice,
                    TotalCost = unitPrice * volume,
                };
            }).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetAnimalIncomeCurrentAsync — mirrors qryDeptIncomeAnimals (Current Data Old Style)
        //
        // Unlike GetAnimalIncomeAsync (snapshot), the Access qryDeptIncomeAnimals current
        // path returns one row per Proj_SubContract entry — NOT grouped by month.
        // Each row exposes Description (AnimalType), AnimalDays, and DailyRate directly
        // from the Proj_SubContract columns.  The snapshot aggregates those into a single
        // per-month TotalCost row; the current query does not.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeAnimal>> GetAnimalIncomeCurrentAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            var rows = await
                (from sc in _context.ProjectSubContracts.AsNoTracking()
                     .Where(s => s.FpsYear == fpsYear
                              && s.AcctCode != null
                              && AnimalAcctCodes.Contains(s.AcctCode)
                              && s.Month.HasValue
                              && (int)s.Month.Value >= monthFrom
                              && (int)s.Month.Value <= monthTo)
                 join proj in _context.Projects.AsNoTracking()
                     .Where(p => p.FpsYear == fpsYear)
                     on sc.Project equals proj.ParentProject
                 join cc in _context.CostCentres.AsNoTracking()
                     .Where(c => c.FpsYear == fpsYear)
                     on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                 from cc in ccJoin.DefaultIfEmpty()
                 select new
                 {
                     proj.ParentProject,
                     proj.OracleProjectCode,
                     proj.SubAccountCode,
                     proj.IsDefraProject,
                     OCC        = cc != null ? (double?)cc.CostCentreNo : null,
                     OPC        = cc != null ? cc.ProfitCentre          : (string?)null,
                     Month      = (int)(sc.Month ?? 0),
                     sc.Description,
                     AnimalDays = (decimal)(sc.AnimalDays ?? 0),
                     DailyRate  = sc.DailyRate ?? 0m,
                     Amount     = sc.Amount    ?? 0m,
                 })
                .ToListAsync();

            var filtered = string.IsNullOrEmpty(project)
                ? rows
                : rows.Where(r => r.ParentProject == project).ToList();

            return filtered
                .Select(r => new DepartmentIncomeAnimal
                {
                    Project           = r.ParentProject,
                    OracleProjectCode = r.OracleProjectCode,
                    SubAccountCode    = r.SubAccountCode,
                    DefraProject      = r.IsDefraProject != 0 ? "Yes" : "No",
                    OPC               = r.OPC,
                    OCC               = r.OCC.HasValue ? ((long)r.OCC.Value).ToString() : null,
                    Month             = r.Month,
                    SPC               = "SSSD",
                    SCC               = "35227",
                    AnimalType        = r.Description,
                    AnimalDays        = r.AnimalDays,
                    Rate              = r.DailyRate,
                    TotalCost         = r.Amount,
                })
                .OrderBy(r => r.Project)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetAdditionalIncomeCurrentAsync — mirrors qryDeptIncomeExceptional (Current Data Old Style)
        //
        // Unlike the snapshot path (GetAdditionalIncomeAsync), Access qryDeptIncomeExceptional
        // has NO HAVING Sum(Amount) > 0 guard — all rows are returned, including those with a
        // negative net TotalCost (e.g. Month 1 reversal row).
        // This is why Current Data (Old Style) shows 4 rows while Snapshot Data shows 3.
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeCurrentAsync(
            string? project, int monthFrom, int monthTo)
        {
            // Mirrors qryDeptIncomeExceptional (Access) — no positivity filter.
            var fpsYear = _requestContext.FpsYear;

            var query =
                from sc in _context.ProjectSubContracts.AsNoTracking()
                    .Where(s => s.FpsYear == fpsYear
                             && s.AcctCode != null
                             && !AnimalAcctCodes.Contains(s.AcctCode)
                             && s.Month.HasValue
                             && (int)s.Month.Value >= monthFrom
                             && (int)s.Month.Value <= monthTo)
                join proj in _context.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on sc.Project equals proj.ParentProject
                join cc in _context.CostCentres.AsNoTracking()
                    .Where(c => c.FpsYear == fpsYear)
                    on proj.CostCentre equals (double?)cc.CostCentreNo into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    proj.IsDefraProject,
                    OCC   = cc != null ? (double?)cc.CostCentreNo : null,
                    OPC   = cc != null ? cc.ProfitCentre          : (string?)null,
                    Month = (int)(sc.Month ?? 0),
                    Amount = sc.Amount ?? 0m,
                };

            if (!string.IsNullOrEmpty(project))
                query = query.Where(r => r.ParentProject == project);

            var grouped = await query.ToListAsync();

            return grouped
                .GroupBy(r => new
                {
                    r.ParentProject,
                    r.OracleProjectCode,
                    r.SubAccountCode,
                    r.IsDefraProject,
                    r.OCC,
                    r.OPC,
                    r.Month,
                })
                .Select(g => new DepartmentIncomeAdditional
                {
                    Project           = g.Key.ParentProject,
                    OracleProjectCode = g.Key.OracleProjectCode,
                    SubAccountCode    = g.Key.SubAccountCode,
                    DefraProject      = g.Key.IsDefraProject != 0 ? "Yes" : "No",
                    OPC               = g.Key.OPC,
                    OCC               = g.Key.OCC.HasValue ? ((long)g.Key.OCC.Value).ToString() : null,
                    Month             = g.Key.Month,
                    TotalCost         = g.Sum(r => r.Amount),   // all rows, including negatives
                })
                .OrderBy(r => r.Project)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTotalsCurrentAsync — mirrors qryDeptIncomeTotals (Current Data Old Style)
        // Sums TotalCost per area across the four current-style queries (no period diff).
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTotals>> GetTotalsCurrentAsync(
            string? project, int monthFrom, int monthTo)
        {
            var timeRows       = await GetTimeIncomeCurrentAsync(project, monthFrom, monthTo);
            var testRows       = await GetTestIncomeCurrentAsync(project, monthFrom, monthTo);
            var animalRows     = await GetAnimalIncomeCurrentAsync(project, monthFrom, monthTo);
            var additionalRows = await GetAdditionalIncomeCurrentAsync(project, monthFrom, monthTo);

            var unionAll =
                timeRows.Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaTime })
                .Concat(testRows.Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaTests }))
                .Concat(animalRows.Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaAnimals }))
                .Concat(additionalRows.Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = AreaProjectSpecifics }))
                .ToList();

            return unionAll
                .GroupBy(r => new { r.Project, r.OracleProjectCode })
                .Select(g =>
                {
                    var timeCost     = g.Where(r => r.Area == AreaTime).Sum(r => (decimal?)r.TotalCost);
                    var testsCost    = g.Where(r => r.Area == AreaTests).Sum(r => (decimal?)r.TotalCost);
                    var animalsCost  = g.Where(r => r.Area == AreaAnimals).Sum(r => (decimal?)r.TotalCost);
                    var projSpecCost = g.Where(r => r.Area == AreaProjectSpecifics).Sum(r => (decimal?)r.TotalCost);

                    return new DepartmentIncomeTotals
                    {
                        Project           = g.Key.Project,
                        OracleProjectCode = g.Key.OracleProjectCode,
                        TotalCosts        = g.Sum(r => r.TotalCost),
                        TimeCost          = timeCost    is null or 0m ? null : timeCost,
                        TestsCost         = testsCost   is null or 0m ? null : testsCost,
                        AnimalsCost       = animalsCost is null or 0m ? null : animalsCost,
                        ProjectSpecificsCost = projSpecCost is null or 0m ? null : projSpecCost,
                    };
                })
                .OrderBy(r => r.Project)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetPeriodsAsync — period/month dropdown lookup
        //
        // Source: sf_Period.frm subform + fps.tblkperiodmonth view
        //   (view definition: JOIN fps.tblperiod ON tblperiod.endperiod = tblperiodmonth.endmonth)
        //   Columns: endmonth (AccntsPeriod), monthno (MonthNumber), periodname (MonthName), fpsyear
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<PeriodLookup>> GetPeriodsAsync(double? accntsPeriod = null)
        {
            var query = _context.PeriodLookups
                .AsNoTracking()
                .Where(p => p.FpsYear == _requestContext.FpsYear);

            if (accntsPeriod.HasValue)
            {
                const double tolerance = 1e-9;
                var lo = accntsPeriod.Value - tolerance;
                var hi = accntsPeriod.Value + tolerance;
                query = query.Where(p => p.AccntsPeriod >= lo && p.AccntsPeriod <= hi);
            }

            var rows = await query
                .OrderBy(p => p.AccntsPeriod)
                .ThenBy(p => p.MonthNumber)
                .ToListAsync();

            // tblkperiodmonth returns multiple monthno rows per AccntsPeriod;
            // the dropdown needs one entry per period (first/lowest monthno row)
            return rows
                .GroupBy(p => p.AccntsPeriod)
                .Select(g => g.First())
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetSnapshotPeriodsAsync — snapshot tab period status lookup
        //
        // Source: tblPeriod (fps.tblperiod)
        //   SELECT PeriodName, EndPeriod, FinalSummariesRun, PeriodLocked
        //   FROM tblPeriod
        //   WHERE fpsyear = <current>
        //   ORDER BY EndPeriod
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<Period>> GetSnapshotPeriodsAsync()
        {
            return await _context.Periods
                .AsNoTracking()
                .Where(p => p.FpsYear == _requestContext.FpsYear)
                .OrderBy(p => p.EndPeriod)
                .ToListAsync();
        }

        public async Task<int> UpdatePeriodLockedAsync(string periodName, bool periodLocked)
        {
            var period = await _context.Periods
                .FirstOrDefaultAsync(p => p.PeriodName == periodName && p.FpsYear == _requestContext.FpsYear);

            if (period is null) return 0;

            period.PeriodLocked = periodLocked ? (short)-1 : (short)0;
            return await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Paged variants
        //   fetch all matching rows, apply filter helpers, apply sort, then base.ApplyPaging.
        // ─────────────────────────────────────────────────────────────────────────────

        public async Task<PagedData<DepartmentIncomeTime>> GetPagedTimeIncomeAsync(
            PaginationParameters<string> query, string? project, int monthFrom, int monthTo)
        {
            var rows = await GetTimeIncomeAsync(project, monthFrom, monthTo);
            var filtered = ApplyTimeFilter(rows, query.Filter);
            var sorted   = ApplyTimeSort(filtered, query.SortBy, query.Descending);
            return base.ApplyPaging(sorted, Math.Max(query.Page, 1), Math.Max(query.PageSize, 10));
        }

        public async Task<PagedData<DepartmentIncomeTest>> GetPagedTestIncomeAsync(
            PaginationParameters<string> query, string? project, int monthFrom, int monthTo)
        {
            var rows = await GetTestIncomeAsync(project, monthFrom, monthTo);
            var filtered = ApplyTestFilter(rows, query.Filter);
            var sorted   = ApplyTestSort(filtered, query.SortBy, query.Descending);
            return base.ApplyPaging(sorted, Math.Max(query.Page, 1), Math.Max(query.PageSize, 10));
        }

        public async Task<PagedData<DepartmentIncomeAnimal>> GetPagedAnimalIncomeAsync(
            PaginationParameters<string> query, string? project, int monthFrom, int monthTo)
        {
            var rows = await GetAnimalIncomeAsync(project, monthFrom, monthTo);
            var filtered = ApplyAnimalFilter(rows, query.Filter);
            var sorted   = ApplyAnimalSort(filtered, query.SortBy, query.Descending);
            return base.ApplyPaging(sorted, Math.Max(query.Page, 1), Math.Max(query.PageSize, 10));
        }

        public async Task<PagedData<DepartmentIncomeAdditional>> GetPagedAdditionalIncomeAsync(
            PaginationParameters<string> query, string? project, int monthFrom, int monthTo)
        {
            var rows = await GetAdditionalIncomeAsync(project, monthFrom, monthTo);
            var filtered = ApplyAdditionalFilter(rows, query.Filter);
            var sorted   = ApplyAdditionalSort(filtered, query.SortBy, query.Descending);
            return base.ApplyPaging(sorted, Math.Max(query.Page, 1), Math.Max(query.PageSize, 10));
        }

        // ── Per-entity filter helpers ─────────────────────────────────────────────

        // Applies a single case-insensitive contains filter for one field.
        // Encapsulates the TryGetValue + IsNullOrWhiteSpace guard so each
        // Apply*Filter method stays flat (no nested boolean conditions).
        private static IEnumerable<T> FilterByField<T>(
            IEnumerable<T> q,
            Dictionary<string, string> f,
            string key,
            Func<T, string?> selector)
        {
            if (!f.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
                return q;
            return q.Where(r => { var val = selector(r); return val != null && val.Contains(v, StringComparison.OrdinalIgnoreCase); });
        }

        private static List<DepartmentIncomeTime> ApplyTimeFilter(List<DepartmentIncomeTime> rows, string? filterJson)
        {
            var f = ParseFilterDict(filterJson);
            if (f is null) return rows;

            IEnumerable<DepartmentIncomeTime> q = rows;
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.Project),           r => r.Project);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.OracleProjectCode), r => r.OracleProjectCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.SubAccountCode),    r => r.SubAccountCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.DefraProject),      r => r.DefraProject);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.OCC),               r => r.OCC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.OPC),               r => r.OPC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.SPC),               r => r.SPC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.SCC),               r => r.SCC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.Name),              r => r.Name);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.GradeCode),         r => r.GradeCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeTime.SpNumber),          r => r.SpNumber);
            return q.ToList();
        }

        private static List<DepartmentIncomeTest> ApplyTestFilter(List<DepartmentIncomeTest> rows, string? filterJson)
        {
            var f = ParseFilterDict(filterJson);
            if (f is null) return rows;

            IEnumerable<DepartmentIncomeTest> q = rows;
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.Project),           r => r.Project);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.OracleProjectCode), r => r.OracleProjectCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.SubAccountCode),    r => r.SubAccountCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.DefraProject),      r => r.DefraProject);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.OCC),               r => r.OCC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.OPC),               r => r.OPC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.SPC),               r => r.SPC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.SCC),               r => r.SCC);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.WorkGroup),         r => r.WorkGroup);
            q = FilterByField(q, f, nameof(DepartmentIncomeTest.TestCode),          r => r.TestCode);
            return q.ToList();
        }

        private static List<DepartmentIncomeAnimal> ApplyAnimalFilter(List<DepartmentIncomeAnimal> rows, string? filterJson)
        {
            var f = ParseFilterDict(filterJson);
            if (f is null) return rows;

            IEnumerable<DepartmentIncomeAnimal> q = rows;
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.Project),           r => r.Project);
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.OracleProjectCode), r => r.OracleProjectCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.SubAccountCode),    r => r.SubAccountCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.DefraProject),      r => r.DefraProject);
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.OCC),               r => r.OCC);
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.OPC),               r => r.OPC);
            q = FilterByField(q, f, nameof(DepartmentIncomeAnimal.AnimalType),        r => r.AnimalType);
            return q.ToList();
        }

        private static List<DepartmentIncomeAdditional> ApplyAdditionalFilter(List<DepartmentIncomeAdditional> rows, string? filterJson)
        {
            var f = ParseFilterDict(filterJson);
            if (f is null) return rows;

            IEnumerable<DepartmentIncomeAdditional> q = rows;
            q = FilterByField(q, f, nameof(DepartmentIncomeAdditional.Project),           r => r.Project);
            q = FilterByField(q, f, nameof(DepartmentIncomeAdditional.OracleProjectCode), r => r.OracleProjectCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeAdditional.SubAccountCode),    r => r.SubAccountCode);
            q = FilterByField(q, f, nameof(DepartmentIncomeAdditional.DefraProject),      r => r.DefraProject);
            q = FilterByField(q, f, nameof(DepartmentIncomeAdditional.OCC),               r => r.OCC);
            q = FilterByField(q, f, nameof(DepartmentIncomeAdditional.OPC),               r => r.OPC);
            return q.ToList();
        }

        // ── Per-entity sort helpers ───────────────────────────────────────────────

        private static List<DepartmentIncomeTime> ApplyTimeSort(List<DepartmentIncomeTime> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return rows;
            return (sortBy, descending) switch
            {
                (nameof(DepartmentIncomeTime.Project),          true)  => rows.OrderByDescending(r => r.Project).ToList(),
                (nameof(DepartmentIncomeTime.OracleProjectCode),true)  => rows.OrderByDescending(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeTime.SubAccountCode),   true)  => rows.OrderByDescending(r => r.SubAccountCode).ToList(),
                (nameof(DepartmentIncomeTime.Month),            true)  => rows.OrderByDescending(r => r.Month).ToList(),
                (nameof(DepartmentIncomeTime.DefraProject),     true)  => rows.OrderByDescending(r => r.DefraProject).ToList(),
                (nameof(DepartmentIncomeTime.Name),             true)  => rows.OrderByDescending(r => r.Name).ToList(),
                (nameof(DepartmentIncomeTime.GradeCode),        true)  => rows.OrderByDescending(r => r.GradeCode).ToList(),
                (nameof(DepartmentIncomeTime.SpNumber),         true)  => rows.OrderByDescending(r => r.SpNumber).ToList(),
                (nameof(DepartmentIncomeTime.ChargeRate),       true)  => rows.OrderByDescending(r => r.ChargeRate).ToList(),
                (nameof(DepartmentIncomeTime.TotalCost),        true)  => rows.OrderByDescending(r => r.TotalCost).ToList(),
                (nameof(DepartmentIncomeTime.Project),          false) => rows.OrderBy(r => r.Project).ToList(),
                (nameof(DepartmentIncomeTime.OracleProjectCode),false) => rows.OrderBy(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeTime.SubAccountCode),   false) => rows.OrderBy(r => r.SubAccountCode).ToList(),
                (nameof(DepartmentIncomeTime.Month),            false) => rows.OrderBy(r => r.Month).ToList(),
                (nameof(DepartmentIncomeTime.DefraProject),     false) => rows.OrderBy(r => r.DefraProject).ToList(),
                (nameof(DepartmentIncomeTime.Name),             false) => rows.OrderBy(r => r.Name).ToList(),
                (nameof(DepartmentIncomeTime.GradeCode),        false) => rows.OrderBy(r => r.GradeCode).ToList(),
                (nameof(DepartmentIncomeTime.SpNumber),         false) => rows.OrderBy(r => r.SpNumber).ToList(),
                (nameof(DepartmentIncomeTime.ChargeRate),       false) => rows.OrderBy(r => r.ChargeRate).ToList(),
                (nameof(DepartmentIncomeTime.TotalCost),        false) => rows.OrderBy(r => r.TotalCost).ToList(),
                _                                                      => rows,
            };
        }

        private static List<DepartmentIncomeTest> ApplyTestSort(List<DepartmentIncomeTest> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return rows;
            return (sortBy, descending) switch
            {
                (nameof(DepartmentIncomeTest.Project),          true)  => rows.OrderByDescending(r => r.Project).ToList(),
                (nameof(DepartmentIncomeTest.OracleProjectCode),true)  => rows.OrderByDescending(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeTest.TestCode),         true)  => rows.OrderByDescending(r => r.TestCode).ToList(),
                (nameof(DepartmentIncomeTest.Month),            true)  => rows.OrderByDescending(r => r.Month).ToList(),
                (nameof(DepartmentIncomeTest.TotalCost),        true)  => rows.OrderByDescending(r => r.TotalCost).ToList(),
                (nameof(DepartmentIncomeTest.Project),          false) => rows.OrderBy(r => r.Project).ToList(),
                (nameof(DepartmentIncomeTest.OracleProjectCode),false) => rows.OrderBy(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeTest.TestCode),         false) => rows.OrderBy(r => r.TestCode).ToList(),
                (nameof(DepartmentIncomeTest.Month),            false) => rows.OrderBy(r => r.Month).ToList(),
                (nameof(DepartmentIncomeTest.TotalCost),        false) => rows.OrderBy(r => r.TotalCost).ToList(),
                _                                                      => rows,
            };
        }

        private static List<DepartmentIncomeAnimal> ApplyAnimalSort(List<DepartmentIncomeAnimal> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return rows;
            return (sortBy, descending) switch
            {
                (nameof(DepartmentIncomeAnimal.Project),          true)  => rows.OrderByDescending(r => r.Project).ToList(),
                (nameof(DepartmentIncomeAnimal.OracleProjectCode),true)  => rows.OrderByDescending(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeAnimal.AnimalType),       true)  => rows.OrderByDescending(r => r.AnimalType).ToList(),
                (nameof(DepartmentIncomeAnimal.Month),            true)  => rows.OrderByDescending(r => r.Month).ToList(),
                (nameof(DepartmentIncomeAnimal.TotalCost),        true)  => rows.OrderByDescending(r => r.TotalCost).ToList(),
                (nameof(DepartmentIncomeAnimal.Project),          false) => rows.OrderBy(r => r.Project).ToList(),
                (nameof(DepartmentIncomeAnimal.OracleProjectCode),false) => rows.OrderBy(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeAnimal.AnimalType),       false) => rows.OrderBy(r => r.AnimalType).ToList(),
                (nameof(DepartmentIncomeAnimal.Month),            false) => rows.OrderBy(r => r.Month).ToList(),
                (nameof(DepartmentIncomeAnimal.TotalCost),        false) => rows.OrderBy(r => r.TotalCost).ToList(),
                _                                                        => rows,
            };
        }

        private static List<DepartmentIncomeAdditional> ApplyAdditionalSort(List<DepartmentIncomeAdditional> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return rows;
            return (sortBy, descending) switch
            {
                (nameof(DepartmentIncomeAdditional.Project),          true)  => rows.OrderByDescending(r => r.Project).ToList(),
                (nameof(DepartmentIncomeAdditional.OracleProjectCode),true)  => rows.OrderByDescending(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeAdditional.Month),            true)  => rows.OrderByDescending(r => r.Month).ToList(),
                (nameof(DepartmentIncomeAdditional.TotalCost),        true)  => rows.OrderByDescending(r => r.TotalCost).ToList(),
                (nameof(DepartmentIncomeAdditional.Project),          false) => rows.OrderBy(r => r.Project).ToList(),
                (nameof(DepartmentIncomeAdditional.OracleProjectCode),false) => rows.OrderBy(r => r.OracleProjectCode).ToList(),
                (nameof(DepartmentIncomeAdditional.Month),            false) => rows.OrderBy(r => r.Month).ToList(),
                (nameof(DepartmentIncomeAdditional.TotalCost),        false) => rows.OrderBy(r => r.TotalCost).ToList(),
                _                                                            => rows,
            };
        }

        private static Dictionary<string, string>? ParseFilterDict(string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return null;
            try { return JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson); }
            catch { return null; }
        }
    }
}
