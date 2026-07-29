using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class DepartmentIncomeRepository : IDepartmentIncomeRepository
    {
        private readonly FpsDbContext _db;
        private readonly IFpsRequestContext _requestContext;

        private static readonly string[] AnimalAcctCodes = { "LargeAnimals", "SmallAnimals", "Mice" };

        public DepartmentIncomeRepository(FpsDbContext db, IFpsRequestContext requestContext)
        {
            _db = db;
            _requestContext = requestContext;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetTimeIncomeAsync — mirrors qryDeptIncomeTime SELECT
        //
        // Access SQL:
        //   FROM tblWGEmployeeMAB
        //     INNER JOIN ((tlkpProject_MAP LEFT JOIN CostCentre ON tlkpProject_MAP.CostCentre = CostCentre.CostCentre)
        //       INNER JOIN (TimeCostCalcsMAP INNER JOIN WorkGroup_MAP ON TimeCostCalcsMAP.WorkGroup = WorkGroup_MAP.WorkGroup)
        //       ON tlkpProject_MAP.ParentProject = TimeCostCalcsMAP.Project)
        //     ON tblWGEmployeeMAB.PACTid = TimeCostCalcsMAP.StaffID
        //   WHERE Class = "Charge" AND Month BETWEEN fnDeptIncomeMonthFrom() AND fnDeptIncomeMonthTo()
        //         AND ParentProject Like nz(fnDeptIncomeProject(),"*")
        //   ORDER BY ParentProject
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTime>> GetTimeIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            // Two-step: (1) DB query returns intermediate anonymous type with raw double? CostCentre values;
            // (2) in-memory projection converts them to strings (ToString is not SQL-translatable).
            var dbQuery =
                from tc in _db.TimeCostCalcs.AsNoTracking()
                    .Where(t => t.FpsYear == fpsYear
                             && t.Class == "Charge"
                             && (int)t.Month >= monthFrom
                             && (int)t.Month <= monthTo)
                join wg in _db.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on tc.WorkGroup equals wg.WorkGroupName
                join proj in _db.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on tc.Project equals proj.ParentProject
                join emp in _db.WorkGroupEmployees.AsNoTracking()
                    .Where(e => e.FpsYear == fpsYear)
                    on tc.StaffId equals emp.PactId
                join wgOwner in _db.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on proj.CostCentre equals wgOwner.CostCentre into wgOwnerJoin
                from wgOwner in wgOwnerJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    TcMonth = tc.Month,
                    proj.IsDefraProject,
                    ProjCostCentre = proj.CostCentre,
                    OPC = wgOwner != null ? wgOwner.ProfitCentre : null,
                    WgProfitCentre = wg.ProfitCentre,
                    WgCostCentre = wg.CostCentre,
                    tc.Name,
                    tc.GradeCode,
                    emp.SpNumber,
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
                OCC = r.ProjCostCentre.HasValue ? ((long)r.ProjCostCentre.Value).ToString() : null,
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
        //   Buyer=JobCode, TestCode=TestCode, UnitPrice=TestPrice
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeTest>> GetTestIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            // Two-step: DB query returns raw anonymous type; in-memory projection formats strings.
            var dbQuery =
                from mo in _db.MonthlyOutputs.AsNoTracking()
                    .Where(m => m.FpsYear == fpsYear
                             && (int)m.Month >= monthFrom
                             && (int)m.Month <= monthTo)
                join wg in _db.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on mo.WorkGroup equals wg.WorkGroupName
                join proj in _db.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on mo.Buyer equals proj.ParentProject
                join tr in _db.TestRequirements.AsNoTracking()
                    .Where(t => t.FpsYear == fpsYear)
                    on new { mo.Buyer, mo.TestCode } equals new { tr.Buyer, tr.TestCode }
                join wgOwner in _db.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on proj.CostCentre equals wgOwner.CostCentre into wgOwnerJoin
                from wgOwner in wgOwnerJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    proj.IsDefraProject,
                    ProjCostCentre = proj.CostCentre,
                    OPC = wgOwner != null ? wgOwner.ProfitCentre : null,
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
                    OCC = r.ProjCostCentre.HasValue ? ((long)r.ProjCostCentre.Value).ToString() : null,
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
            var fpsYear = _requestContext.FpsYear;

            // Raw fetch first — fnAnimalDesc/fnAnimalDays parsing cannot be translated to SQL
            var subContracts = await
                (from sc in _db.ProjectSubContracts.AsNoTracking()
                    .Where(s => s.FpsYear == fpsYear
                             && s.AcctCode != null
                             && AnimalAcctCodes.Contains(s.AcctCode)
                             && s.Month.HasValue
                             && (int)s.Month.Value >= monthFrom
                             && (int)s.Month.Value <= monthTo)
                 join proj in _db.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on sc.Project equals proj.ParentProject
                 join wgOwner in _db.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on proj.CostCentre equals wgOwner.CostCentre into wgOwnerJoin
                 from wgOwner in wgOwnerJoin.DefaultIfEmpty()
                 select new
                 {
                     proj.ParentProject,
                     proj.OracleProjectCode,
                     proj.SubAccountCode,
                     proj.IsDefraProject,
                     proj.CostCentre,
                     OPC = wgOwner != null ? wgOwner.ProfitCentre : (string?)null,
                     Month = (int)(sc.Month ?? 0),
                     Description = sc.Description,
                     Amount = sc.Amount ?? 0m,
                 }).ToListAsync();

            if (!string.IsNullOrEmpty(project))
                subContracts = subContracts.Where(r => r.ParentProject == project).ToList();

            var animalRates = await _db.Animals.AsNoTracking()
                .Where(a => a.FpsYear == fpsYear)
                .ToDictionaryAsync(a => a.AnimalType, a => a.DailyRate ?? 0m);

            var result = subContracts
                .Select(r =>
                {
                    var animalType = ParseAnimalDesc(r.Description);
                    var animalDays = ParseAnimalDays(r.Description);
                    animalRates.TryGetValue(animalType ?? string.Empty, out var rate);

                    return new DepartmentIncomeAnimal
                    {
                        Project = r.ParentProject,
                        OracleProjectCode = r.OracleProjectCode,
                        SubAccountCode = r.SubAccountCode,
                        DefraProject = r.IsDefraProject != 0 ? "Yes" : "No",
                        OPC = r.OPC,
                        OCC = r.CostCentre != null ? ((double)r.CostCentre).ToString("F0") : null,
                        Month = r.Month,
                        SPC = "SSSD",
                        SCC = "35227",
                        AnimalType = animalType,
                        AnimalDays = animalDays,
                        Rate = rate,
                        TotalCost = r.Amount,
                    };
                })
                .OrderBy(r => r.Project)
                .ToList();

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetAdditionalIncomeAsync — mirrors qryDeptIncomeExceptional SELECT/GROUP BY
        //
        // Access SQL:
        //   FROM (CostCentre RIGHT JOIN tlkpProject_MAP ON CostCentre.CostCentre = tlkpProject_MAP.CostCentre)
        //     INNER JOIN Proj_SubContract ON tlkpProject_MAP.ParentProject = Proj_SubContract.Project
        //   WHERE AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice")
        //   GROUP BY ParentProject, OracleProjectCode, SubAccountCode, IIf(...), OPC, OCC, Month
        //   HAVING ParentProject Like nz(fnDeptIncomeProject(),"*")
        //     AND Month BETWEEN fnDeptIncomeMonthFrom() AND fnDeptIncomeMonthTo()
        //   ORDER BY ParentProject
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeAsync(
            string? project, int monthFrom, int monthTo)
        {
            var fpsYear = _requestContext.FpsYear;

            var query =
                from sc in _db.ProjectSubContracts.AsNoTracking()
                    .Where(s => s.FpsYear == fpsYear
                             && s.AcctCode != null
                             && !AnimalAcctCodes.Contains(s.AcctCode)
                             && s.Month.HasValue
                             && (int)s.Month.Value >= monthFrom
                             && (int)s.Month.Value <= monthTo)
                join proj in _db.Projects.AsNoTracking()
                    .Where(p => p.FpsYear == fpsYear)
                    on sc.Project equals proj.ParentProject
                join wgOwner in _db.Workgroups.AsNoTracking()
                    .Where(w => w.FpsYear == fpsYear)
                    on proj.CostCentre equals wgOwner.CostCentre into wgOwnerJoin
                from wgOwner in wgOwnerJoin.DefaultIfEmpty()
                select new
                {
                    proj.ParentProject,
                    proj.OracleProjectCode,
                    proj.SubAccountCode,
                    proj.IsDefraProject,
                    proj.CostCentre,
                    OPC = wgOwner != null ? wgOwner.ProfitCentre : (string?)null,
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
                    r.CostCentre,
                    r.OPC,
                    r.Month,
                })
                .Select(g => new DepartmentIncomeAdditional
                {
                    Project = g.Key.ParentProject,
                    OracleProjectCode = g.Key.OracleProjectCode,
                    SubAccountCode = g.Key.SubAccountCode,
                    DefraProject = g.Key.IsDefraProject != 0 ? "Yes" : "No",
                    OPC = g.Key.OPC,
                    OCC = g.Key.CostCentre != null ? ((double)g.Key.CostCentre).ToString("F0") : null,
                    Month = g.Key.Month,
                    TotalCost = g.Sum(r => r.Amount),
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
                .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = "Time" })
                .Concat(testRows
                    .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = "Tests" }))
                .Concat(animalRows
                    .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = "Animals" }))
                .Concat(additionalRows
                    .Select(r => new { r.Project, r.OracleProjectCode, r.TotalCost, Area = "Project-specifics" }))
                .ToList();

            var result = unionAll
                .GroupBy(r => new { r.Project, r.OracleProjectCode })
                .Select(g =>
                {
                    var timeCost = g.Where(r => r.Area == "Time").Sum(r => (decimal?)r.TotalCost);
                    var testsCost = g.Where(r => r.Area == "Tests").Sum(r => (decimal?)r.TotalCost);
                    var animalsCost = g.Where(r => r.Area == "Animals").Sum(r => (decimal?)r.TotalCost);
                    var projSpecCost = g.Where(r => r.Area == "Project-specifics").Sum(r => (decimal?)r.TotalCost);

                    return new DepartmentIncomeTotals
                    {
                        Project = g.Key.Project,
                        OracleProjectCode = g.Key.OracleProjectCode,
                        TotalCosts = g.Sum(r => r.TotalCost),
                        TimeCost = timeCost == 0m ? null : timeCost,
                        TestsCost = testsCost == 0m ? null : testsCost,
                        AnimalsCost = animalsCost == 0m ? null : animalsCost,
                        ProjectSpecificsCost = projSpecCost == 0m ? null : projSpecCost,
                    };
                })
                .OrderBy(r => r.Project)
                .ToList();

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // GetPeriodsAsync — period/month dropdown lookup
        //
        // Source: sf_Period.frm subform + fps.tblkperiodmonth view
        //   (view definition: JOIN fps.tblperiod ON tblperiod.endperiod = tblperiodmonth.endmonth)
        //   Columns: endmonth (AccntsPeriod), monthno (MonthNumber), periodname (MonthName), fpsyear
        // ─────────────────────────────────────────────────────────────────────────────
        public async Task<List<PeriodLookup>> GetPeriodsAsync()
        {
            var fpsYear = _requestContext.FpsYear;

            return await _db.PeriodLookups
                .AsNoTracking()
                .OrderBy(p => p.AccntsPeriod)
                .ToListAsync();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // VBA helper function ports
        // ─────────────────────────────────────────────────────────────────────────────

        //   If InStr(d, " x ") > 0 Then fnAnimalDesc = Left(d, p - 1) Else fnAnimalDesc = d
        private static string? ParseAnimalDesc(string? description)
        {
            if (string.IsNullOrEmpty(description))
                return description;

            var idx = description.IndexOf(" x ", StringComparison.Ordinal);
            if (idx > 0)
                return description[..idx];

            return description;
        }

        //   p = InStr(d, " x "), q = InStr(d, "@")
        //   If p > 0 And q > 0 Then fnAnimalDays = Mid(d, p + 3, q - p - 4) Else Null
        private static decimal ParseAnimalDays(string? description)
        {
            if (string.IsNullOrEmpty(description))
                return 0m;

            var p = description.IndexOf(" x ", StringComparison.Ordinal);
            var q = description.IndexOf("@", StringComparison.Ordinal);

            if (p >= 0 && q > p)
            {
                var startIndex = p + 3;           // skip " x "
                var length = q - p - 4;           // VBA: q - p - 4 (one-based offset difference → zero-based length)
                if (length > 0 && startIndex + length <= description.Length)
                {
                    var daysStr = description.Substring(startIndex, length).Trim();
                    if (decimal.TryParse(daysStr, out var days))
                        return days;
                }
            }

            return 0m;
        }
    }
}
