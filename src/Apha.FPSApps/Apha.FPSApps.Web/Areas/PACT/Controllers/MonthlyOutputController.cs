using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using PactMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.PactMonthlyOutputDto;
using StagingMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.StagingMonthlyOutputDto;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class MonthlyOutputController : Controller
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _monthlyOutputService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;

        public MonthlyOutputController(
            IMapper mapper,
            IPactMonthlyOutputService monthlyOutputService,
            IWorkGroupService workGroupService,
            IMonthService monthService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _monthlyOutputService = monthlyOutputService;
            _workGroupService = workGroupService;
            _monthService = monthService;
            _excelExportService = excelExportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var liveGrid = await BuildLiveGridAsync(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                null, null, null, null);

            var stagingGrid = await BuildStagingGridAsync(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                null);

            var viewModel = new MonthlyOutputViewModel
            {
                WorkGroupOptions = await GetWorkGroupOptionsAsync(),
                TestCodeOptions = new List<SelectListItem>(),
                BuyerOptions = new List<SelectListItem>(),
                MonthOptions = await GetMonthOptionsAsync(),
                LiveGrid = liveGrid,
                StagingGrid = stagingGrid,
                LiveTotalVolume = liveGrid.Total,
                StagingTotalVolume = stagingGrid.Total
            };

            return View(viewModel);
        }

        // ── Grid loaders ─────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadLiveGrid(
            PaginationFilter<string> request,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildLiveGridAsync(request, workGroup, testCode, buyer, month);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStagingGrid(PaginationFilter<string> request, bool? passed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildStagingGridAsync(request, passed);
            return PartialView("_DataGrid", grid);
        }

        // ── Lookup endpoints (AJAX) ───────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetTestCodesByWorkGroup(string? workGroup)
        {
            if (string.IsNullOrWhiteSpace(workGroup))
                return Json(Array.Empty<object>());

            var response = await _monthlyOutputService.GetLiveAsync(
                new QueryParameters<string> { Page = -1 }, workGroup, null, null, null);

            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var result = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.TestCode))
                .Select(x => x.TestCode!)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new { value = x, text = x })
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBuyersByTestCode(string? workGroup, string? testCode)
        {
            if (string.IsNullOrWhiteSpace(workGroup) || string.IsNullOrWhiteSpace(testCode))
                return Json(Array.Empty<object>());

            var response = await _monthlyOutputService.GetLiveAsync(
                new QueryParameters<string> { Page = -1 }, workGroup, testCode, null, null);

            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var result = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.Buyer))
                .Select(x => new { buyer = x.Buyer!, testCode = x.TestCode ?? string.Empty })
                .DistinctBy(x => x.buyer)
                .OrderBy(x => x.buyer)
                .Select(x => new { value = x.buyer, text = x.buyer, testCode = x.testCode })
                .ToList();

            return Json(result);
        }

        // ── Live record edit ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetLiveRecord(string testCode, string buyer, double month, string workGroup)
        {
            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();

            var response = await _monthlyOutputService.GetLiveByKeyAsync(testCode, buyer, month, workGroup);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<MonthlyOutputLiveItem>(response.Data);
            return PartialView("_EditMonthlyOutputLive", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLiveRecord([FromBody] MonthlyOutputLiveItem model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data." });

            var validationErrors = await ValidateLiveRecordAsync(model);
            if (validationErrors.Count > 0)
                return Json(new { success = false, message = "Validation failed.", errors = validationErrors });

            var dto = _mapper.Map<PactMonthlyOutputDto>(model);

            var keyParts = (model.CompositeKey ?? string.Empty).Split('|');
            dto.OriginalTestCode  = keyParts.ElementAtOrDefault(0);
            dto.OriginalBuyer     = keyParts.ElementAtOrDefault(1);
            dto.OriginalMonth     = double.TryParse(keyParts.ElementAtOrDefault(2), out var km) ? km : 0;
            dto.OriginalWorkGroup = keyParts.ElementAtOrDefault(3);

            var response = await _monthlyOutputService.UpdateLiveAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Monthly output record updated successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update monthly output record.",
                errors = response.Errors?.Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "Validation error" })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLiveRecord(string testCode, string buyer, double month, string workGroup)
        {
            var response = await _monthlyOutputService.DeleteLiveAsync(testCode, buyer, month, workGroup);
            return Json(new { success = response.Success && response.Data });
        }

        // ── Staging record edit ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetStagingRecord(int id)
        {
            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();

            var response = await _monthlyOutputService.GetStagingByIdAsync(id);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<StagingMonthlyOutputItem>(response.Data);
            return PartialView("_AddEditStagingMonthlyOutput", model);
        }

        [HttpGet]
        public async Task<IActionResult> AddStagingRecord()
        {
            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();
            return PartialView("_AddEditStagingMonthlyOutput", new StagingMonthlyOutputItem());
        }

        [HttpPost]
        public async Task<IActionResult> SaveStagingRecord([FromBody] StagingMonthlyOutputItem model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data." });

            var dto = _mapper.Map<StagingMonthlyOutputDto>(model);

            ApiResponseDto<StagingMonthlyOutputDto> response = model.Id == 0
                ? await _monthlyOutputService.CreateStagingAsync(dto)
                : await _monthlyOutputService.UpdateStagingAsync(model.Id, dto);

            if (!response.Success)
                return Json(new
                {
                    success = false,
                    message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save staging record."
                });

            return Json(new
            {
                success = true,
                message = model.Id == 0 ? "Staging record added successfully." : "Staging record updated successfully."
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStagingRecord(int id)
        {
            var response = await _monthlyOutputService.DeleteStagingAsync(id);
            return Json(new { success = response.Success && response.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllStagingRecords()
        {
            var response = await _monthlyOutputService.DeleteAllStagingByUserAsync();
            return Json(new { success = response.Success && response.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFailedStagingRecords()
        {
            var response = await _monthlyOutputService.DeleteFailedStagingByUserAsync();
            return Json(new
            {
                success = response.Success && response.Data,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete failed imported records."
            });
        }

        // ── Export ────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ExportStaging(bool? passed)
        {
            var response = await _monthlyOutputService.GetStagingAsync(new QueryParameters<string> { Page = -1 }, passed);
            if (!response.Success || response.Data == null)
                return NotFound();

            var rows = _mapper.Map<List<StagingMonthlyOutputExportItem>>(response.Data);
            var excelBytes = _excelExportService.ExportToExcel(rows, "MonthlyOutput");
            var fileName = $"ExportedOP_{DateTime.Now:ddMMyyyy}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        // ── Import / Validate / Make Live ─────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select an Excel file to import." });

            var response = await _monthlyOutputService.ImportMonthlyOutputAsync(file);
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    importedCount = response.Data.ImportedCount,
                    passedCount = response.Data.PassedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Import failed." });
        }

        [HttpPost]
        public async Task<IActionResult> Validate()
        {
            var response = await _monthlyOutputService.ValidateStagingAsync();
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    passedCount = response.Data.PassedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Validation failed." });
        }

        [HttpPost]
        public async Task<IActionResult> MakeLive()
        {
            var response = await _monthlyOutputService.MakeLiveAsync();
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    processedCount = response.Data.ProcessedCount,
                    importedCount = response.Data.ImportedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Make live failed.",
                errors = response.Errors
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private async Task<DataGridConfig<MonthlyOutputLiveItem>> BuildLiveGridAsync(
            PaginationFilter<string> request,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            var hasAnyFilter = !string.IsNullOrWhiteSpace(workGroup)
                || !string.IsNullOrWhiteSpace(testCode)
                || !string.IsNullOrWhiteSpace(buyer)
                || month.HasValue;

            var currentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];

            List<MonthlyOutputLiveItem> items;
            decimal total;
            PaginationModel pagination;

            if (!hasAnyFilter)
            {
                items = [];
                total = 0;
                pagination = new PaginationModel
                {
                    TotalRecords = 0,
                    PageNumber = request.Page > 0 ? request.Page : 1,
                    PageSize = request.PageSize > 0 ? request.PageSize : 10
                };
            }
            else
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _monthlyOutputService.GetLiveAsync(query, workGroup, testCode, buyer, month);

                items = response.Success && response.Data != null
                    ? _mapper.Map<List<MonthlyOutputLiveItem>>(response.Data)
                    : [];
                total = response.Total;
                pagination = response.Pagination != null
                    ? _mapper.Map<PaginationModel>(response.Pagination)
                    : new PaginationModel();
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<MonthlyOutputLiveItem>
            {
                GridId = "monthlyOutputLiveGrid",
                Title = "Monthly Output",
                AllowAdd = false,
                AllowDelete = false,
                ShowCheckboxColumn = false,
                KeyProperty = "CompositeKey",
                EditFunction = "editMonthlyOutputLive",
                BindGridUrl = "/PACT/MonthlyOutput/LoadLiveGrid",
                ExtraFilterMethod = "getMonthlyOutputLiveFilters",
                Data = items,
                Total = total,
                Columns = GridDataProvider.GetColumnsDefination<MonthlyOutputLiveItem>(null),
                Pagination = pagination,
                CurrentFilters = currentFilters
            };
        }

        private async Task<DataGridConfig<StagingMonthlyOutputItem>> BuildStagingGridAsync(
            PaginationFilter<string> request,
            bool? passed)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _monthlyOutputService.GetStagingAsync(query, passed);
            var items = response.Success && response.Data != null
                ? _mapper.Map<List<StagingMonthlyOutputItem>>(response.Data)
                : [];
            var total = response.Total;
            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StagingMonthlyOutputItem>
            {
                GridId = "monthlyOutputStagingGrid",
                Title = "Imported Output Records",
                AllowExport = false,
                ShowCheckboxColumn = false,
                KeyProperty = "Id",
                AddFunction = "addStagingMonthlyOutput",
                EditFunction = "editStagingMonthlyOutput",
                DeleteFunction = "deleteStagingMonthlyOutput",
                BindGridUrl = "/PACT/MonthlyOutput/LoadStagingGrid",
                ExtraFilterMethod = "getMonthlyOutputStagingFilters",
                Data = items,
                Total = total,
                Columns = GridDataProvider.GetColumnsDefination<StagingMonthlyOutputItem>(null),
                Pagination = pagination,
                CurrentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? []
            };
        }

        private async Task<List<SelectListItem>> GetWorkGroupOptionsAsync()
        {
            var response = await _workGroupService.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x.WorkGroupName).Select(x => new SelectListItem(x.WorkGroupName, x.WorkGroupName)).ToList()
                : [];
        }

        private async Task<List<SelectListItem>> GetMonthOptionsAsync()
        {
            var response = await _monthService.GetAllMonthsAsync();
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x.Monthnumber).Select(x => new SelectListItem(x.Monthname, x.Monthnumber.ToString())).ToList()
                : [];
        }

        private async Task<List<object>> ValidateLiveRecordAsync(MonthlyOutputLiveItem model)
        {
            var errors = new List<object>();

            if (model.Volume is null || model.Volume <= 0)
                errors.Add(new { field = "Volume", message = "Volume must be greater than zero." });

            var workGroup = model.WorkGroup?.Trim();
            if (string.IsNullOrWhiteSpace(workGroup))
            {
                errors.Add(new { field = "WorkGroup", message = "The work group name is blank." });
            }
            else
            {
                var workGroups = await GetWorkGroupOptionsAsync();
                if (!workGroups.Any(x => string.Equals(x.Value, workGroup, StringComparison.OrdinalIgnoreCase)))
                    errors.Add(new { field = "WorkGroup", message = $"The work group name is invalid: {workGroup}" });
            }

            if (string.IsNullOrWhiteSpace(model.TestCode?.Trim()))
                errors.Add(new { field = "TestCode", message = "The test code is blank." });

            if (string.IsNullOrWhiteSpace(model.Buyer?.Trim()))
                errors.Add(new { field = "Buyer", message = "The buyer is blank." });

            var validMonths = await GetMonthOptionsAsync();
            var monthValue = model.Month.ToString("0");
            if (!validMonths.Any(x => string.Equals(x.Value, monthValue, StringComparison.OrdinalIgnoreCase)))
                errors.Add(new { field = "Month", message = $"The month number is invalid: {model.Month}" });

            return errors;
        }
    }
}
