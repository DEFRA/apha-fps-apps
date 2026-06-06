using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class BudgetResourceLevelController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IPurchasesService _purchasesService;
        private readonly IProfitCentreService _profitCentreService;

        public BudgetResourceLevelController(
            IMapper mapper,
            IWorkGroupService workGroupService,
            IBudgetBidsService budgetBidsService,
            IPurchasesService purchasesService,
            IProfitCentreService profitCentreService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
            _budgetBidsService = budgetBidsService;
            _purchasesService = purchasesService;
            _profitCentreService = profitCentreService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var viewModel = new BudgetResourceLevelViewModel
            {
                SelectedProfitCentre = profitCentre ?? string.Empty
            };

            await PopulateProfitCentresAsync(viewModel);

            var defaultRequest = new PaginationFilter<string> { Filter = "{}", SortBy = string.Empty, Descending = false, Page = 1, PageSize = 10 };

            viewModel.WorkGroupGrid = await GetWorkGroupGridConfigAsync(defaultRequest, profitCentre);
            viewModel.BudgetBidsGrid = await GetBudgetBidsGridConfigAsync(defaultRequest, null);
            viewModel.PurchasesGrid = await GetPurchasesGridConfigAsync(defaultRequest, null, null);

            return View(viewModel);
        }

        // ─────────────── LOAD GRID ENDPOINTS ───────────────

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadWorkGroupGrid(
            PaginationFilter<string> request, string? profitCentre = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var gridConfig = await GetWorkGroupGridConfigAsync(request, profitCentre);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadBudgetBidsGrid(
            PaginationFilter<string> request, string? workgroup = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var gridConfig = await GetBudgetBidsGridConfigAsync(request, workgroup);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadPurchasesGrid(
            PaginationFilter<string> request, string? workgroup = null, string? account = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var gridConfig = await GetPurchasesGridConfigAsync(request, workgroup, account);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<WorkGroupItem>> GetWorkGroupGridConfigAsync(
            PaginationFilter<string> request, string? profitCentre)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var allItems = new List<WorkGroupItem>();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var response = await _workGroupService.GetWorkGroupsAsync(profitCentre);
                if (response.Success && response.Data != null)
                    allItems = response.Data.Select(d => new WorkGroupItem { WorkgroupName = d.WorkgroupName, WorkGroup = d.WorkgroupName }).ToList();
            }

            var totalRecords = allItems.Count;
            var pageSize     = request.PageSize > 0 ? request.PageSize : 10;
            var pageNumber   = request.Page > 0 ? request.Page : 1;
            var pagedItems   = allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new DataGridConfig<WorkGroupItem>
            {
                GridId            = "workGroupGrid",
                Title             = "Work Groups",
                ShowCheckboxColumn = false,
                ShowPagination    = true,
                KeyProperty       = "WorkgroupName",
                AllowAdd          = false,
                AddFunction       = null,
                AllowEdit         = false,
                EditFunction      = null,
                AllowDelete       = false,
                DeleteFunction    = null,
                ExtraFilterMethod = "getWorkGroupExtraFilters",
                BindGridUrl       = "/FPS/BudgetResourceLevel/LoadWorkGroupGrid",
                Data              = pagedItems,
                Columns           = GridDataProvider.GetColumnsDefination<WorkGroupItem>(null),
                Pagination        = new PaginationModel
                {
                    TotalRecords  = totalRecords,
                    PageNumber    = pageNumber,
                    PageSize      = pageSize,
                    SortColumn    = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters    = filterDict
            };
        }

        private async Task<DataGridConfig<BudgetResourceCentreLevelItem>> GetBudgetBidsGridConfigAsync(
            PaginationFilter<string> request, string? workgroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var allItems = new List<BudgetResourceCentreLevelItem>();

            if (!string.IsNullOrWhiteSpace(workgroup))
            {
                var response = await _budgetBidsService.GetBidViewAsync(workgroup);
                if (response.Success && response.Data != null)
                {
                    var accountResult = await _budgetBidsService.GetAccountCategoriesAsync();
                    var accountList = accountResult.Data?.Select(a => new SelectListItem
                    {
                        Value = a.AccShortName,
                        Text  = string.IsNullOrWhiteSpace(a.AccountDescription) ? a.AccShortName : $"{a.AccShortName} - {a.AccountDescription}"
                    }).ToList() ?? new List<SelectListItem>();

                    allItems = response.Data.Select(d => new BudgetResourceCentreLevelItem
                    {
                        WorkgroupName = d.WorkgroupName,
                        Account       = d.Account,
                        GenBid        = d.GenBid,
                        AccountList   = accountList
                    }).ToList();
                }
            }

            var totalRecords = allItems.Count;
            var pageSize     = request.PageSize > 0 ? request.PageSize : 10;
            var pageNumber   = request.Page > 0 ? request.Page : 1;
            var pagedItems   = allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new DataGridConfig<BudgetResourceCentreLevelItem>
            {
                GridId            = "budgetBidsGrid",
                Title             = "Budget Bids",
                ShowCheckboxColumn = false,
                ShowPagination    = true,
                KeyProperty       = "Account",
                AllowAdd          = true,
                AddFunction       = "addBudgetBid",
                AllowEdit         = true,
                EditFunction      = "editBudgetBid",
                AllowDelete       = true,
                DeleteFunction    = "deleteBudgetBid",
                ExtraFilterMethod = "getBudgetBidExtraFilters",
                BindGridUrl       = "/FPS/BudgetResourceLevel/LoadBudgetBidsGrid",
                Data              = pagedItems,
                Columns           = GridDataProvider.GetColumnsDefination<BudgetResourceCentreLevelItem>(null),
                Pagination        = new PaginationModel
                {
                    TotalRecords  = totalRecords,
                    PageNumber    = pageNumber,
                    PageSize      = pageSize,
                    SortColumn    = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters    = filterDict
            };
        }

        private async Task<DataGridConfig<PurchaseItem>> GetPurchasesGridConfigAsync(
            PaginationFilter<string> request, string? workgroup, string? account)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var allItems = new List<PurchaseItem>();

            if (!string.IsNullOrWhiteSpace(workgroup) && !string.IsNullOrWhiteSpace(account))
            {
                var response = await _purchasesService.GetPurchasesAsync(workgroup, account);
                if (response.Success && response.Data != null)
                {
                    allItems = response.Data.Select(d => new PurchaseItem
                    {
                        WorkgroupName   = d.WorkgroupName,
                        Account         = d.Account,
                        ItemDescription = d.ItemDescription,
                        Amount          = d.Amount
                    }).ToList();
                }
            }

            var totalRecords = allItems.Count;
            var pageSize     = request.PageSize > 0 ? request.PageSize : 10;
            var pageNumber   = request.Page > 0 ? request.Page : 1;
            var pagedItems   = allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new DataGridConfig<PurchaseItem>
            {
                GridId            = "purchasesGrid",
                Title             = "Purchases",
                ShowCheckboxColumn = false,
                ShowPagination    = true,
                KeyProperty       = "ItemDescription",
                AllowAdd          = true,
                AddFunction       = "addPurchase",
                AllowEdit         = true,
                EditFunction      = "editPurchase",
                AllowDelete       = true,
                DeleteFunction    = "deletePurchase",
                ExtraFilterMethod = "getPurchaseExtraFilters",
                BindGridUrl       = "/FPS/BudgetResourceLevel/LoadPurchasesGrid",
                Data              = pagedItems,
                Columns           = GridDataProvider.GetColumnsDefination<PurchaseItem>(null),
                Pagination        = new PaginationModel
                {
                    TotalRecords  = totalRecords,
                    PageNumber    = pageNumber,
                    PageSize      = pageSize,
                    SortColumn    = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters    = filterDict
            };
        }

        // ─────────────── BUDGET BIDS CRUD ───────────────

        [HttpGet]
        public async Task<IActionResult> CreateBudgetBid(string workgroupName)
        {
            var model = new BudgetResourceCentreLevelItem { WorkgroupName = workgroupName };
            await PopulateBidDropdownsAsync(model);
            return PartialView("_AddEditBudgetResourceLevel", BudgetResourceLevelItem.ForBudgetBid(model));
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudgetBid([FromBody] BudgetResourceCentreLevelItem model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors  = ModelState.Where(kvp => kvp.Value!.Errors.Any())
                                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = new BidDto { WorkgroupName = model.WorkgroupName, Account = model.Account, GenBid = model.GenBid };
            var result = await _budgetBidsService.CreateBidAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Budget bid created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create budget bid.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditBudgetBid(string workgroupName, string account)
        {
            var result = await _budgetBidsService.GetBidByIdAsync(workgroupName, account);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to retrieve bid details." });

            var model = new BudgetResourceCentreLevelItem
            {
                WorkgroupName = result.Data.WorkgroupName,
                Account       = result.Data.Account,
                GenBid        = result.Data.GenBid
            };
            await PopulateBidDropdownsAsync(model);
            return PartialView("_AddEditBudgetResourceLevel", BudgetResourceLevelItem.ForBudgetBid(model));
        }

        [HttpPost]
        public async Task<IActionResult> EditBudgetBid([FromBody] BudgetResourceCentreLevelItem model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors  = ModelState.Where(kvp => kvp.Value!.Errors.Any())
                                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = new BidDto { WorkgroupName = model.WorkgroupName, Account = model.Account, GenBid = model.GenBid };
            var result = await _budgetBidsService.UpdateBidAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Budget bid updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update budget bid.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBudgetBid(string workgroupName, string account)
        {
            var dto = new BidDto { WorkgroupName = workgroupName, Account = account };
            var result = await _budgetBidsService.DeleteBidAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Budget bid deleted successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete budget bid." });
        }

        // ─────────────── PURCHASES CRUD ───────────────

        [HttpGet]
        public IActionResult CreatePurchase(string workgroupName, string account)
        {
            var model = new PurchaseItem { WorkgroupName = workgroupName, Account = account };
            return PartialView("_AddEditBudgetResourceLevel", BudgetResourceLevelItem.ForPurchase(model));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromBody] PurchaseItem model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors  = ModelState.Where(kvp => kvp.Value!.Errors.Any())
                                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = new PurchaseDto { WorkgroupName = model.WorkgroupName, Account = model.Account, ItemDescription = model.ItemDescription, Amount = model.Amount };
            var result = await _purchasesService.CreatePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Purchase created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create purchase.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditPurchase(string workgroupName, string account, string itemDescription)
        {
            var result = await _purchasesService.GetPurchaseByIdAsync(workgroupName, account, itemDescription);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to retrieve purchase details." });

            var model = new PurchaseItem
            {
                WorkgroupName   = result.Data.WorkgroupName,
                Account         = result.Data.Account,
                ItemDescription = result.Data.ItemDescription,
                Amount          = result.Data.Amount
            };
            return PartialView("_AddEditBudgetResourceLevel", BudgetResourceLevelItem.ForPurchase(model));
        }

        [HttpPost]
        public async Task<IActionResult> EditPurchase([FromBody] PurchaseItem model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors  = ModelState.Where(kvp => kvp.Value!.Errors.Any())
                                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = new PurchaseDto
            {
                WorkgroupName      = model.WorkgroupName,
                Account            = model.Account,
                ItemDescription    = model.ItemDescription,
                Amount             = model.Amount,
                OldItemDescription = model.ItemDescription
            };
            var result = await _purchasesService.UpdatePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Purchase updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update purchase.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePurchase(string workgroupName, string account, string itemDescription)
        {
            var dto = new PurchaseDto { WorkgroupName = workgroupName, Account = account, ItemDescription = itemDescription };
            var result = await _purchasesService.DeletePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Purchase deleted successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete purchase." });
        }

        // ─────────────── EXCEL EXPORT ───────────────

        /// <summary>
        /// Exports a crosstab (pivot) of all Budget Bids for the given profit centre to Excel.
       
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string profitCentre)
        {
            // 1. Fetch all workgroups for the profit centre
            var wgResponse = await _workGroupService.GetWorkGroupsAsync(profitCentre);
            var workgroups = wgResponse.Success && wgResponse.Data != null
                ? wgResponse.Data.Select(w => w.WorkgroupName).OrderBy(w => w).ToList()
                : new List<string>();

            // 2. Fetch all account categories (all rows of the crosstab)
            var accountResponse = await _budgetBidsService.GetAccountCategoriesAsync();
            var accounts = accountResponse.Success && accountResponse.Data != null
                ? accountResponse.Data.OrderBy(a => a.AccShortName).ToList()
                : new List<AccountCategoryDto>();

            // 3. Fetch all bids for every workgroup
            //    pivot[account][workgroup] = GenBid
            var pivot = new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);

            foreach (var wg in workgroups)
            {
                var bidResponse = await _budgetBidsService.GetBidViewAsync(wg);
                if (!bidResponse.Success || bidResponse.Data == null) continue;

                foreach (var bid in bidResponse.Data)
                {
                    if (!pivot.TryGetValue(bid.Account, out var wgDict))
                    {
                        wgDict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                        pivot[bid.Account] = wgDict;
                    }
                    wgDict[bid.WorkgroupName] = bid.GenBid;
                }
            }

            // 4. Build the workbook
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("BudgetBidsCrosstab");

            // ── Header row ──────────────────────────────────────────────────────
            // Col 1: AccShortName  Col 2: Row Summary  Col 3: <>  Col 4+: each workgroup
            int headerRow = 1;
            ws.Cell(headerRow, 1).Value = "AccShortName";
            ws.Cell(headerRow, 2).Value = "Row Summary";
            ws.Cell(headerRow, 3).Value = "<>";

            for (int i = 0; i < workgroups.Count; i++)
                ws.Cell(headerRow, 4 + i).Value = workgroups[i];

            var headerRange = ws.Range(headerRow, 1, headerRow, 3 + workgroups.Count);
            headerRange.Style.Font.Bold = true;

            // ── Data rows ────────────────────────────────────────────────────────
            int dataRow = 2;
            foreach (var account in accounts)
            {
                pivot.TryGetValue(account.AccShortName, out var wgValues);

                // Row Summary = sum of all bids for this account across all workgroups
                decimal rowSummary = wgValues?.Values.Sum() ?? 0m;

                ws.Cell(dataRow, 1).Value = account.AccShortName;

                if (rowSummary != 0)
                {
                    ws.Cell(dataRow, 2).Value = rowSummary;
                    ws.Cell(dataRow, 2).Style.NumberFormat.Format = "£#,##0.00;-£#,##0.00";
                }

                // Col 3 (<>) is intentionally blank (matches Access crosstab)

                for (int i = 0; i < workgroups.Count; i++)
                {
                    if (wgValues != null && wgValues.TryGetValue(workgroups[i], out var bidAmount) && bidAmount != 0)
                    {
                        ws.Cell(dataRow, 4 + i).Value = bidAmount;
                        ws.Cell(dataRow, 4 + i).Style.NumberFormat.Format = "£#,##0.00;-£#,##0.00";
                    }
                }

                dataRow++;
            }

            // ── Auto-fit columns ─────────────────────────────────────────────────
            ws.Columns().AdjustToContents();

            // ── Stream to response ───────────────────────────────────────────────
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = "qryBidxCrosstab.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ─────────────── HELPERS ───────────────

        private async Task PopulateProfitCentresAsync(BudgetResourceLevelViewModel model)
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            model.ProfitCentreList = result.Data == null ? new List<SelectListItem>() :
                result.Data.Select(p => new SelectListItem
                {
                    Value    = p.ProfitCentreId,
                    Text     = p.ProfitCentreId,
                    Selected = string.Equals(model.SelectedProfitCentre, p.ProfitCentreId, StringComparison.OrdinalIgnoreCase)
                }).ToList();
        }

        private async Task PopulateBidDropdownsAsync(BudgetResourceCentreLevelItem model)
        {
            var accountResult = await _budgetBidsService.GetAccountCategoriesAsync();
            model.AccountList = accountResult.Data == null ? new List<SelectListItem>() :
                accountResult.Data.Select(a => new SelectListItem
                {
                    Value    = a.AccShortName,
                    Text     = string.IsNullOrWhiteSpace(a.AccountDescription) ? a.AccShortName : $"{a.AccShortName} - {a.AccountDescription}",
                    Selected = string.Equals(model.Account, a.AccShortName, StringComparison.OrdinalIgnoreCase)
                }).ToList();
            model.AccountFullList = accountResult.Data ?? new List<AccountCategoryDto>();
        }
    }
}