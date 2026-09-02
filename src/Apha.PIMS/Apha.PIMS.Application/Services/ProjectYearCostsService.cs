using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using ClosedXML.Excel;

namespace Apha.PIMS.Application.Services
{
    public class ProjectYearCostsService : IProjectYearCostsService
    {
        private readonly IProjectYearCostsRepository _repository;
        private readonly IMapper _mapper;

        public ProjectYearCostsService(IProjectYearCostsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AdditionalCostDto>> GetAdditionalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<ProjSubContract> paged = await _repository.GetAdditionalActualsAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AdditionalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<AdditionalCostDto>> GetAdditionalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<AdditionalCosts> paged = await _repository.GetAdditionalPlansAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AdditionalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<AnimalCostDto>> GetAnimalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<ProjSubContract> paged = await _repository.GetAnimalActualsAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AnimalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<AnimalCostDto>> GetAnimalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<ProjectAnimalPlan> paged = await _repository.GetAnimalPlansAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<AnimalCostDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<TestCostDto>> GetTestPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<TestReqmt> paged = await _repository.GetTestPlansAsync(project, year, paging);
            List<TestCostDto> items = paged.Data.Select(t => new TestCostDto
            {
                Year       = t.Year,
                Buyer      = t.Buyer,
                TestCode   = t.Testcode,
                UnitPrice  = t.Unitprice,
                NoRequired = t.Norequired,
                Cost       = t.Norequired.HasValue && t.Unitprice.HasValue
                                 ? t.Unitprice.Value * (decimal)t.Norequired.Value
                                 : null
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        public async Task<PaginatedResult<TestCostDto>> GetTestActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<(MonthlyOutput Output, TestReqmt Reqmt)> paged =
                await _repository.GetTestActualsAsync(project, year, paging);
            List<TestCostDto> items = paged.Data.Select(x => new TestCostDto
            {
                Year      = x.Output.Year,
                Buyer     = x.Output.Buyer,
                TestCode  = x.Output.Testcode,
                UnitPrice = x.Reqmt.Unitprice,
                Month     = x.Output.Month,
                WorkGroup = x.Output.Workgroup,
                Volume    = x.Output.Volume,
                Charge    = x.Output.Volume.HasValue && x.Reqmt.Unitprice.HasValue
                                ? x.Reqmt.Unitprice.Value * (decimal)x.Output.Volume.Value
                                : null
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        private static PaginatedResult<TDto> BuildResult<TDto>(List<TDto> items, PaginationData pd)
        {
            return new PaginatedResult<TDto>(items, new PaginationDto
            {
                PageNumber   = pd.PageNumber,
                PageSize     = pd.PageSize,
                TotalPages   = pd.TotalPages,
                TotalRecords = pd.TotalRecords
            });
        }

        public async Task<FpsYearTotalsDto?> GetFpsYearTotalsAsync(string project, short year)
        {
            var entity = await _repository.GetFpsYearTotalsAsync(project, year);
            return entity == null ? null : _mapper.Map<FpsYearTotalsDto>(entity);
        }

        public async Task<PaginatedResult<StaffCostDto>> GetStaffPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<ProjectStaffPlan> paged = await _repository.GetStaffPlansAsync(project, year, paging);
            List<StaffCostDto> items = paged.Data.Select(s => new StaffCostDto
            {
                Year         = s.Year,
                ParentProject = s.Parentproject,
                WgGrade      = s.Workgroupgrade,
                Name         = s.Name,
                PlannedHours = s.Plannedhours,
                Rate         = s.Rate,
                Cost         = s.Cost
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        public async Task<PaginatedResult<StaffCostDto>> GetStaffActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<TimeCostCalcs> paged = await _repository.GetStaffActualsAsync(project, year, paging);
            List<StaffCostDto> items = paged.Data.Select(s => new StaffCostDto
            {
                JobCode    = s.Jobcode,
                Name       = s.Name,
                WorkGroup  = s.Workgroup,
                GradeCode  = s.Gradecode,
                Month      = s.Month,
                Time       = s.Time,
                ChargeRate = s.Chargerate,
                ActualCost = s.Time.HasValue && s.Chargerate.HasValue
                                 ? Math.Round((decimal)s.Time.Value * s.Chargerate.Value, 2)
                                 : null
            }).ToList();
            return BuildResult(items, paged.PaginationData);
        }

        public async Task<ProjectYearDetailsDto> GetProjectYearDetailsAsync(string project, short year)
        {
            Projects? entity = await _repository.GetProjectYearDetailsAsync(project, year);
            return entity is null ? new ProjectYearDetailsDto() : _mapper.Map<ProjectYearDetailsDto>(entity);
        }

        public async Task<PaginatedResult<PactPayDto>> GetPactPayAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<PactPayCalc> paged = await _repository.GetPactPayAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<PactPayDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<PaginatedResult<MonthlyPactDto>> GetMonthlyPactDataAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            PagedData<ProjectMonthFinal> paged = await _repository.GetMonthlyPactDataAsync(project, year, paging);
            return BuildResult(_mapper.Map<List<MonthlyPactDto>>(paged.Data), paged.PaginationData);
        }

        public async Task<byte[]> ExportProjectYearCostsToExcelAsync(string project, short year)
        {
            PaginationParameters<string> allRecords = new() { Page = 1, PageSize = int.MaxValue };

            PagedData<ProjectStaffPlan> staffPlansTask = await _repository.GetStaffPlansAsync(project, year, allRecords);
            PagedData<TimeCostCalcs> staffActualsTask = await _repository.GetStaffActualsAsync(project, year, allRecords);
            PagedData<TestReqmt> testPlansTask = await _repository.GetTestPlansAsync(project, year, allRecords);
            PagedData<(MonthlyOutput Output, TestReqmt Reqmt)> testActualsTask = await _repository.GetTestActualsAsync(project, year, allRecords);
            PagedData<ProjectAnimalPlan> animalPlansTask = await _repository.GetAnimalPlansAsync(project, year, allRecords);
            PagedData<ProjSubContract> animalActualsTask = await _repository.GetAnimalActualsAsync(project, year, allRecords);
            PagedData<AdditionalCosts> additionalPlansTask = await _repository.GetAdditionalPlansAsync(project, year, allRecords);
            PagedData<ProjSubContract> additionalActualsTask = await _repository.GetAdditionalActualsAsync(project, year, allRecords);

            using var workbook = new XLWorkbook();

            BuildStaffPlanSheet(workbook, staffPlansTask.Data.ToList());
            BuildStaffActualsSheet(workbook, staffActualsTask.Data.ToList());
            BuildTestPlanSheet(workbook, testPlansTask.Data.ToList());
            BuildTestActualsSheet(workbook, testActualsTask.Data.ToList());
            BuildAnimalPlanSheet(workbook, animalPlansTask.Data.ToList());
            BuildAnimalActualsSheet(workbook, animalActualsTask.Data.ToList());
            BuildAdditionalPlanSheet(workbook, additionalPlansTask.Data.ToList());
            BuildAdditionalActualsSheet(workbook, additionalActualsTask.Data.ToList());

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void ApplyHeaderStyle(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1d70b8");
            cell.Style.Font.FontColor = XLColor.White;
        }

        private const string PoundCurrencyFormat = "£#,##0.00";

        private static void ApplyTotalsRowStyle(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f3f2f1");
        }

        private static void ApplyPoundCurrencyFormat(IXLCell cell)
        {
            cell.Style.NumberFormat.Format = PoundCurrencyFormat;
        }

        private static void BuildStaffPlanSheet(XLWorkbook wb, List<ProjectStaffPlan> data)
        {
            var ws = wb.Worksheets.Add("StaffPlan");
            string[] headers = ["WG Grade", "Name", "Planned Hours", "Rate", "Cost"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var s in data)
            {
                ws.Cell(row, 1).Value = s.Workgroupgrade;
                ws.Cell(row, 2).Value = s.Name;
                ws.Cell(row, 3).Value = s.Plannedhours ?? 0d;
                ws.Cell(row, 4).Value = (double)(s.Rate ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 4));
                ws.Cell(row, 5).Value = (double)(s.Cost ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 5));
                row++;
            }

            decimal totalCost = data.Sum(x => x.Cost ?? 0m);
            var totalLabelCell = ws.Cell(row, 4);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 5);
            totalValCell.Value = (double)totalCost;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildStaffActualsSheet(XLWorkbook wb, List<TimeCostCalcs> data)
        {
            var ws = wb.Worksheets.Add("StaffActuals");
            string[] headers = ["Job Code", "Name", "Work Group", "Grade Code", "Month", "Time (hrs)", "Charge Rate", "Actual Cost"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var s in data)
            {
                decimal actualCost = s.Time.HasValue && s.Chargerate.HasValue
                    ? Math.Round((decimal)s.Time.Value * s.Chargerate.Value, 2)
                    : 0m;
                ws.Cell(row, 1).Value = s.Jobcode;
                ws.Cell(row, 2).Value = s.Name;
                ws.Cell(row, 3).Value = s.Workgroup;
                ws.Cell(row, 4).Value = s.Gradecode;
                ws.Cell(row, 5).Value = s.Month;
                ws.Cell(row, 6).Value = s.Time ?? 0d;
                ws.Cell(row, 7).Value = (double)(s.Chargerate ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 7));
                ws.Cell(row, 8).Value = (double)actualCost;
                ApplyPoundCurrencyFormat(ws.Cell(row, 8));
                row++;
            }

            decimal totalActualCost = data.Sum(s =>
                s.Time.HasValue && s.Chargerate.HasValue
                    ? Math.Round((decimal)s.Time.Value * s.Chargerate.Value, 2)
                    : 0m);
            var totalLabelCell = ws.Cell(row, 7);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 8);
            totalValCell.Value = (double)totalActualCost;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildTestPlanSheet(XLWorkbook wb, List<TestReqmt> data)
        {
            var ws = wb.Worksheets.Add("TestPlan");
            string[] headers = ["Test Code", "Buyer", "Unit Price", "No. Required", "Cost"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var t in data)
            {
                decimal cost = t.Norequired.HasValue && t.Unitprice.HasValue
                    ? t.Unitprice.Value * (decimal)t.Norequired.Value
                    : 0m;
                ws.Cell(row, 1).Value = t.Testcode;
                ws.Cell(row, 2).Value = t.Buyer;
                ws.Cell(row, 3).Value = (double)(t.Unitprice ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 3));
                ws.Cell(row, 4).Value = t.Norequired ?? 0d;
                ws.Cell(row, 5).Value = (double)cost;
                ApplyPoundCurrencyFormat(ws.Cell(row, 5));
                row++;
            }

            decimal totalCost = data.Sum(t =>
                t.Norequired.HasValue && t.Unitprice.HasValue
                    ? t.Unitprice.Value * (decimal)t.Norequired.Value
                    : 0m);
            var totalLabelCell = ws.Cell(row, 4);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 5);
            totalValCell.Value = (double)totalCost;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildTestActualsSheet(XLWorkbook wb, List<(MonthlyOutput Output, TestReqmt Reqmt)> data)
        {
            var ws = wb.Worksheets.Add("TestActuals");
            string[] headers = ["Test Code", "Buyer", "Work Group", "Month", "Volume", "Unit Price", "Charge"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var (o, r) in data)
            {
                decimal charge = o.Volume.HasValue && r.Unitprice.HasValue
                    ? r.Unitprice.Value * (decimal)o.Volume.Value
                    : 0m;
                ws.Cell(row, 1).Value = o.Testcode;
                ws.Cell(row, 2).Value = o.Buyer;
                ws.Cell(row, 3).Value = o.Workgroup;
                ws.Cell(row, 4).Value = o.Month;
                ws.Cell(row, 5).Value = o.Volume ?? 0d;
                ws.Cell(row, 6).Value = (double)(r.Unitprice ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 6));
                ws.Cell(row, 7).Value = (double)charge;
                ApplyPoundCurrencyFormat(ws.Cell(row, 7));
                row++;
            }

            decimal totalCharge = data.Sum(x =>
                x.Output.Volume.HasValue && x.Reqmt.Unitprice.HasValue
                    ? x.Reqmt.Unitprice.Value * (decimal)x.Output.Volume.Value
                    : 0m);
            var totalLabelCell = ws.Cell(row, 6);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 7);
            totalValCell.Value = (double)totalCharge;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildAnimalPlanSheet(XLWorkbook wb, List<ProjectAnimalPlan> data)
        {
            var ws = wb.Worksheets.Add("AnimalPlan");
            string[] headers = ["Animal Type", "Number of Days", "Number of Animals", "Rate", "Cost"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var a in data)
            {
                ws.Cell(row, 1).Value = a.Animaltype;
                ws.Cell(row, 2).Value = a.Numberofdays ?? 0d;
                ws.Cell(row, 3).Value = a.Numberofanimals ?? 0d;
                ws.Cell(row, 4).Value = (double)(a.Rate ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 4));
                ws.Cell(row, 5).Value = (double)(a.Cost ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 5));
                row++;
            }

            decimal totalCost = data.Sum(x => x.Cost ?? 0m);
            var totalLabelCell = ws.Cell(row, 4);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 5);
            totalValCell.Value = (double)totalCost;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildAnimalActualsSheet(XLWorkbook wb, List<ProjSubContract> data)
        {
            var ws = wb.Worksheets.Add("AnimalActuals");

            string[] headers = ["Month", "Acct Code", "Description", "Daily Rate", "Animal Days", "Amount"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var a in data)
            {
                ws.Cell(row, 1).Value = a.Month ?? 0d;
                ws.Cell(row, 2).Value = a.Acctcode;
                ws.Cell(row, 3).Value = a.Description;
                ws.Cell(row, 4).Value = (double)(a.DailyRate ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 4));
                ws.Cell(row, 5).Value = a.AnimalDays ?? 0;
                ws.Cell(row, 6).Value = (double)(a.Amount ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 6));
                row++;
            }

            decimal totalAmount = data.Sum(x => x.Amount ?? 0m);
            var totalLabelCell = ws.Cell(row, 5);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 6);
            totalValCell.Value = (double)totalAmount;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildAdditionalPlanSheet(XLWorkbook wb, List<AdditionalCosts> data)
        {
            var ws = wb.Worksheets.Add("AdditionalPlan");
            string[] headers = ["Job Code", "Account", "Description", "Item Cost"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var a in data)
            {
                ws.Cell(row, 1).Value = a.Jobcode;
                ws.Cell(row, 2).Value = a.Account;
                ws.Cell(row, 3).Value = a.Description;
                ws.Cell(row, 4).Value = (double)a.Itemcost;
                ApplyPoundCurrencyFormat(ws.Cell(row, 4));
                row++;
            }

            decimal totalCost = data.Sum(x => x.Itemcost);
            var totalLabelCell = ws.Cell(row, 3);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 4);
            totalValCell.Value = (double)totalCost;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }

        private static void BuildAdditionalActualsSheet(XLWorkbook wb, List<ProjSubContract> data)
        {
            var ws = wb.Worksheets.Add("AdditionalActuals");

            string[] headers = ["Month", "Acct Code", "Description", "Supplier", "Amount"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            int row = 2;
            foreach (var a in data)
            {
                ws.Cell(row, 1).Value = a.Month ?? 0d;
                ws.Cell(row, 2).Value = a.Acctcode;
                ws.Cell(row, 3).Value = a.Description;
                ws.Cell(row, 4).Value = a.Supplier;
                ws.Cell(row, 5).Value = (double)(a.Amount ?? 0m);
                ApplyPoundCurrencyFormat(ws.Cell(row, 5));
                row++;
            }

            decimal totalAmount = data.Sum(x => x.Amount ?? 0m);
            var totalLabelCell = ws.Cell(row, 4);
            totalLabelCell.Value = "Total";
            ApplyTotalsRowStyle(totalLabelCell);
            var totalValCell = ws.Cell(row, 5);
            totalValCell.Value = (double)totalAmount;
            ApplyPoundCurrencyFormat(totalValCell);
            ApplyTotalsRowStyle(totalValCell);

            ws.Columns().AdjustToContents();
        }
    }
}
