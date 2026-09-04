using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.ExcelExport;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]

    public class BudgetResourceLevelController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IPurchasesService _purchasesService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IExcelExportService _excelExportService;

        public BudgetResourceLevelController(
            IMapper mapper,
            IWorkGroupService workGroupService,
            IBudgetBidsService budgetBidsService,
            IPurchasesService purchasesService,
            IProfitCentreService profitCentreService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _workGroupService = workGroupService;
            _budgetBidsService = budgetBidsService;
            _purchasesService = purchasesService;
            _profitCentreService = profitCentreService;
            _excelExportService = excelExportService ?? throw new ArgumentNullException(nameof(excelExportService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var viewModel = new BudgetResourceLevelViewModel
            {
                SelectedProfitCentre = profitCentre ?? string.Empty
            };

            await PopulateProfitCentresAsync(viewModel);

            var defaultRequest     = new PaginationFilter<string> { Filter = "{}", SortBy = string.Empty, Descending = false, Page = 1, PageSize = 10 };
            var workGroupRequest   = new PaginationFilter<string> { Filter = "{}", SortBy = string.Empty, Descending = false, Page = 1, PageSize = 5 };

            viewModel.WorkGroupGrid   = await GetWorkGroupGridConfigAsync(workGroupRequest, profitCentre);
            viewModel.BudgetBidsGrid  = await GetBudgetBidsGridConfigAsync(defaultRequest, null);
            viewModel.PurchasesGrid   = await GetPurchasesGridConfigAsync(defaultRequest, null, null);

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

        private async Task<DataGridConfig<WorkGroupItem>> GetWorkGroupGridConfigAsync(PaginationFilter<string> request, string? profitCentre)
        {
            var filterDict     = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            queryParameters.PageSize = request.PageSize > 0 ? request.PageSize : 5;

            var pagedItems = new List<WorkGroupItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var pagedData = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetPagedAsync(queryParameters, profitCentre);
                if (pagedData.Success && pagedData.Data != null)
                {
                    pagedItems = pagedData.Data.Select((WorkGroupViewDto d) => new WorkGroupItem
                    {
                        WorkGroupName = d.WorkGroupName,
                        WorkGroup     = d.WorkGroupName,
                        ProfitCentre  = d.ProfitCentre,
                        Description   = d.Description,
                        FpsYear       = d.FpsYear,
                        Dt2Username   = d.Dt2Username,
                        UserEmail     = d.UserEmail
                    }).ToList();

                    paginationModel = pagedData.Pagination == null
                        ? new PaginationModel()
                        : _mapper.Map<PaginationModel>(pagedData.Pagination);
                }
            }

            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<WorkGroupItem>
            {
                GridId             = "workGroupGrid",
                Title              = "Work Groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "WorkGroupName",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getWorkGroupExtraFilters",
                BindGridUrl        = Url.Action(nameof(LoadWorkGroupGrid), "BudgetResourceLevel", new { area = "FPS" })!,
                Data               = pagedItems,
                Columns            = GridDataProvider.GetColumnsDefination<WorkGroupItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        private async Task<DataGridConfig<BudgetResourceCentreLevelItem>> GetBudgetBidsGridConfigAsync(
            PaginationFilter<string> request, string? workgroup)
        {
            var filterDict      = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var pagedItems      = new List<BudgetResourceCentreLevelItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(workgroup))
            {
                var pagedData = await _budgetBidsService.GetBidViewPagedAsync(queryParameters, workgroup);
                if (pagedData.Success && pagedData.Data != null)
                {
                    var accountResult = await _budgetBidsService.GetAccountCategoriesAsync();
                    var accountList = accountResult.Data?.Select(a => new SelectListItem
                    {
                        Value = a.AccShortName,
                        Text  = string.IsNullOrWhiteSpace(a.AccountDescription) ? a.AccShortName : $"{a.AccShortName} - {a.AccountDescription}"
                    }).ToList() ?? new List<SelectListItem>();

                    pagedItems = pagedData.Data.Select(d => new BudgetResourceCentreLevelItem
                    {
                        WorkGroupName = d.WorkGroupName,
                        Account       = d.Account,
                        GenBid        = d.GenBid,
                        AccountList   = accountList
                    }).ToList();

                    paginationModel = pagedData.Pagination == null
                        ? new PaginationModel()
                        : _mapper.Map<PaginationModel>(pagedData.Pagination);
                }
            }

            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<BudgetResourceCentreLevelItem>
            {
                GridId             = "budgetBidsGrid",
                Title              = "Budget Bids",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Account",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editBudgetBid",
                AllowDelete        = true,
                DeleteFunction     = "deleteBudgetBid",
                ExtraFilterMethod  = "getBudgetBidExtraFilters",
                BindGridUrl        = Url.Action(nameof(LoadBudgetBidsGrid), "BudgetResourceLevel", new { area = "FPS" })!,
                Data               = pagedItems,
                Columns            = GridDataProvider.GetColumnsDefination<BudgetResourceCentreLevelItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        private async Task<DataGridConfig<PurchaseItem>> GetPurchasesGridConfigAsync(
            PaginationFilter<string> request, string? workgroup, string? account)
        {
            var filterDict      = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var pagedItems      = new List<PurchaseItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(workgroup) && !string.IsNullOrWhiteSpace(account))
            {
                var pagedData = await _purchasesService.GetPurchasesPagedAsync(queryParameters, workgroup, account);
                if (pagedData.Success && pagedData.Data != null)
                {
                    pagedItems = pagedData.Data.Select(d => new PurchaseItem
                    {
                        WorkGroupName   = d.WorkGroupName,
                        Account         = d.Account,
                        ItemDescription = d.ItemDescription,
                        Amount          = d.Amount
                    }).ToList();

                    paginationModel = pagedData.Pagination == null
                        ? new PaginationModel()
                        : _mapper.Map<PaginationModel>(pagedData.Pagination);
                }
            }

            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<PurchaseItem>
            {
                GridId             = "purchasesGrid",
                Title              = "Purchases",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "ItemDescription",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editPurchase",
                AllowDelete        = true,
                DeleteFunction     = "deletePurchase",
                ExtraFilterMethod  = "getPurchaseExtraFilters",
                BindGridUrl        = Url.Action(nameof(LoadPurchasesGrid), "BudgetResourceLevel", new { area = "FPS" })!,
                Data               = pagedItems,
                Columns            = GridDataProvider.GetColumnsDefination<PurchaseItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ─────────────── BUDGET BIDS CRUD ───────────────

        [HttpGet]
        public async Task<IActionResult> CreateBudgetBid(string WorkGroupName)
        {
            var model = new BudgetResourceCentreLevelItem { WorkGroupName = WorkGroupName };
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

            var dto = new BidDto { WorkGroupName = model.WorkGroupName, Account = model.Account, GenBid = model.GenBid };
            var result = await _budgetBidsService.CreateBidAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Budget bid created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create budget bid.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditBudgetBid(string WorkGroupName, string account)
        {
            var result = await _budgetBidsService.GetBidByIdAsync(WorkGroupName, account);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to retrieve bid details." });

            var model = new BudgetResourceCentreLevelItem
            {
                WorkGroupName = result.Data.WorkGroupName,
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

            var dto = new BidDto { WorkGroupName = model.WorkGroupName, Account = model.Account, GenBid = model.GenBid };
            var result = await _budgetBidsService.UpdateBidAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Budget bid updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update budget bid.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBudgetBid(string WorkGroupName, string account)
        {
            var dto = new BidDto { WorkGroupName = WorkGroupName, Account = account };
            var result = await _budgetBidsService.DeleteBidAsync(dto);

            if (result.Success)
                return Json(new { success = true, message = "Budget bid deleted successfully." });

            var firstError = result.Errors?.FirstOrDefault();
            var errorMessage = firstError?.Code is "BUSINESS_RULE_VIOLATION" or "DB_POSTGRES_ERROR"
                ? "The record cannot be deleted because it is being used elsewhere."
                : firstError?.Message ?? "Failed to delete budget bid.";

            return Json(new { success = false, message = errorMessage });
        }

        // ─────────────── PURCHASES CRUD ───────────────

        [HttpGet]
        public IActionResult CreatePurchase(string WorkGroupName, string account)
        {
            var model = new PurchaseItem { WorkGroupName = WorkGroupName, Account = account };
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

            var dto = new PurchaseDto { WorkGroupName = model.WorkGroupName, Account = model.Account, ItemDescription = model.ItemDescription, Amount = model.Amount };
            var result = await _purchasesService.CreatePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Purchase created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create purchase.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditPurchase(string WorkGroupName, string account, string itemDescription)
        {
            var result = await _purchasesService.GetPurchaseByIdAsync(WorkGroupName, account, itemDescription);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to retrieve purchase details." });

            var model = new PurchaseItem
            {
                WorkGroupName   = result.Data.WorkGroupName,
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
                WorkGroupName      = model.WorkGroupName,
                Account            = model.Account,
                ItemDescription    = model.ItemDescription,
                Amount             = model.Amount,
                OldItemDescription = model.OldItemDescription
            };
            var result = await _purchasesService.UpdatePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Purchase updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update purchase.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePurchase(string WorkGroupName, string account, string itemDescription)
        {
            var dto = new PurchaseDto { WorkGroupName = WorkGroupName, Account = account, ItemDescription = itemDescription };
            var result = await _purchasesService.DeletePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Purchase deleted successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete purchase." });
        }

        // ─────────────── EXCEL EXPORT ───────────────

        /// <summary>
        /// Exports all Budget Bids for the given profit centre to Excel as a cross-tab workbook.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string profitCentre, int year)
        {
            var wgResponse = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
            var workgroups = wgResponse.Success && wgResponse.Data != null
                ? wgResponse.Data.Select(w => w.WorkGroupName).OrderBy(w => w).ToList()
                : new List<string>();

            var allBids = new List<BidViewDto>();
            foreach (var wg in workgroups)
            {
                var bidResponse = await _budgetBidsService.GetBidViewAsync(wg);
                if (!bidResponse.Success || bidResponse.Data == null) continue;
                allBids.AddRange(bidResponse.Data);
            }

            // Build lookup: [account][workgroup] = GenBid
            var bidLookup = allBids
                .GroupBy(b => b.Account)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(b => b.WorkGroupName, b => b.GenBid));

            // Use the full account category master list as row keys.
            // Fall back to bid-derived accounts if the categories call fails.
            var categoriesResponse = await _budgetBidsService.GetAccountCategoriesAsync();
            var accounts = categoriesResponse.Success && categoriesResponse.Data?.Count > 0
                ? categoriesResponse.Data.Select(a => a.AccShortName).OrderBy(a => a).ToList()
                : allBids.Select(b => b.Account).Distinct().OrderBy(a => a).ToList();

            var bytes = _excelExportService.BuildBudgetBidsCrosstabExcel(accounts, workgroups, bidLookup);

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"qryBidxCrosstab_{year}.xlsx");
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
