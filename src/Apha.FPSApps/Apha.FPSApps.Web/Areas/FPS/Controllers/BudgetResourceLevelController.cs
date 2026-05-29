using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class BudgetResourceLevelController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IBudgetResourceLevelService _service;

        public BudgetResourceLevelController(IMapper mapper, IBudgetResourceLevelService service)
        {
            _mapper = mapper;
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var viewModel = new BudgetResourceLevelViewModel
            {
                SelectedProfitCentre = profitCentre ?? string.Empty
            };

            await PopulateProfitCentresAsync(viewModel);

            // WorkGroup grid — read-only selector
            viewModel.WorkGroupGrid = new DataGridConfig<WorkGroupItem>
            {
                GridId             = "workGroupGrid",
                Title              = "Work Groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "WorkgroupName",
                AllowAdd           = false,
                AddFunction        = null,
                AllowEdit          = false,
                EditFunction       = null,
                AllowDelete        = false,
                DeleteFunction     = null,
                ExtraFilterMethod  = "getWorkGroupExtraFilters",
                BindGridUrl        = "/FPS/BudgetResourceLevel/LoadWorkGroupGrid",
                Data               = new List<WorkGroupItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<WorkGroupItem>(),
                Pagination         = new PaginationModel()
            };

            // Budget Bids grid — add/edit/delete
            viewModel.BudgetBidsGrid = new DataGridConfig<BudgetResourceCentreLevelItem>
            {
                GridId             = "budgetBidsGrid",
                Title              = "Budget Bids",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Account",
                AllowAdd           = true,
                AddFunction        = "addBudgetBid",
                AllowEdit          = true,
                EditFunction       = "editBudgetBid",
                AllowDelete        = true,
                DeleteFunction     = "deleteBudgetBid",
                ExtraFilterMethod  = "getBudgetBidExtraFilters",
                BindGridUrl        = "/FPS/BudgetResourceLevel/LoadBudgetBidsGrid",
                Data               = new List<BudgetResourceCentreLevelItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<BudgetResourceCentreLevelItem>(),
                Pagination         = new PaginationModel()
            };

            // Purchases grid — add/edit/delete
            viewModel.PurchasesGrid = new DataGridConfig<PurchaseItem>
            {
                GridId             = "purchasesGrid",
                Title              = "Purchases",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "ItemDescription",
                AllowAdd           = true,
                AddFunction        = "addPurchase",
                AllowEdit          = true,
                EditFunction       = "editPurchase",
                AllowDelete        = true,
                DeleteFunction     = "deletePurchase",
                ExtraFilterMethod  = "getPurchaseExtraFilters",
                BindGridUrl        = "/FPS/BudgetResourceLevel/LoadPurchasesGrid",
                Data               = new List<PurchaseItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<PurchaseItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        // ─────────────── LOAD GRID ENDPOINTS ───────────────

        [HttpPost]
        public async Task<IActionResult> LoadWorkGroupGrid(
            PaginationFilter<string> request, string? profitCentre = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var gridConfig = await GetWorkGroupGridConfigAsync(request, profitCentre);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadBudgetBidsGrid(
            PaginationFilter<string> request, string? workgroup = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var gridConfig = await GetBudgetBidsGridConfigAsync(request, workgroup);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
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
            var items = new List<WorkGroupItem>();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var response = await _service.GetWorkGroupsAsync(profitCentre);
                if (response.Success && response.Data != null)
                    items = response.Data.Select(d => new WorkGroupItem { WorkgroupName = d.WorkgroupName, WorkGroup = d.WorkgroupName }).ToList();
            }

            return new DataGridConfig<WorkGroupItem>
            {
                GridId             = "workGroupGrid",
                Title              = "Work Groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "WorkgroupName",
                AllowAdd           = false,
                AddFunction        = null,
                AllowEdit          = false,
                EditFunction       = null,
                AllowDelete        = false,
                DeleteFunction     = null,
                ExtraFilterMethod  = "getWorkGroupExtraFilters",
                BindGridUrl        = "/FPS/BudgetResourceLevel/LoadWorkGroupGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<WorkGroupItem>(null),
                Pagination         = new PaginationModel { TotalRecords = items.Count, PageNumber = 1, PageSize = items.Count > 0 ? items.Count : 10 },
                CurrentFilters     = filterDict
            };
        }

        private async Task<DataGridConfig<BudgetResourceCentreLevelItem>> GetBudgetBidsGridConfigAsync(
            PaginationFilter<string> request, string? workgroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var items = new List<BudgetResourceCentreLevelItem>();

            if (!string.IsNullOrWhiteSpace(workgroup))
            {
                var response = await _service.GetBidViewAsync(workgroup);
                if (response.Success && response.Data != null)
                {
                    var accountResult = await _service.GetAccountCategoriesAsync();
                    var accountList = accountResult.Data?.Select(a => new SelectListItem
                    {
                        Value = a.AccShortName,
                        Text  = string.IsNullOrWhiteSpace(a.AccountDescription) ? a.AccShortName : $"{a.AccShortName} - {a.AccountDescription}"
                    }).ToList() ?? new List<SelectListItem>();

                    items = response.Data.Select(d => new BudgetResourceCentreLevelItem
                    {
                        WorkgroupName = d.WorkgroupName,
                        Account       = d.Account,
                        GenBid        = d.GenBid,
                        AccountList   = accountList
                    }).ToList();
                }
            }

            return new DataGridConfig<BudgetResourceCentreLevelItem>
            {
                GridId             = "budgetBidsGrid",
                Title              = "Budget Bids",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Account",
                AllowAdd           = true,
                AddFunction        = "addBudgetBid",
                AllowEdit          = true,
                EditFunction       = "editBudgetBid",
                AllowDelete        = true,
                DeleteFunction     = "deleteBudgetBid",
                ExtraFilterMethod  = "getBudgetBidExtraFilters",
                BindGridUrl        = "/FPS/BudgetResourceLevel/LoadBudgetBidsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<BudgetResourceCentreLevelItem>(null),
                Pagination         = new PaginationModel { TotalRecords = items.Count, PageNumber = 1, PageSize = items.Count > 0 ? items.Count : 10 },
                CurrentFilters     = filterDict
            };
        }

        private async Task<DataGridConfig<PurchaseItem>> GetPurchasesGridConfigAsync(
            PaginationFilter<string> request, string? workgroup, string? account)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var items = new List<PurchaseItem>();

            if (!string.IsNullOrWhiteSpace(workgroup) && !string.IsNullOrWhiteSpace(account))
            {
                var response = await _service.GetPurchasesAsync(workgroup, account);
                if (response.Success && response.Data != null)
                {
                    items = response.Data.Select(d => new PurchaseItem
                    {
                        WorkgroupName   = d.WorkgroupName,
                        Account         = d.Account,
                        ItemDescription = d.ItemDescription,
                        Amount          = d.Amount
                    }).ToList();
                }
            }

            return new DataGridConfig<PurchaseItem>
            {
                GridId             = "purchasesGrid",
                Title              = "Purchases",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "ItemDescription",
                AllowAdd           = true,
                AddFunction        = "addPurchase",
                AllowEdit          = true,
                EditFunction       = "editPurchase",
                AllowDelete        = true,
                DeleteFunction     = "deletePurchase",
                ExtraFilterMethod  = "getPurchaseExtraFilters",
                BindGridUrl        = "/FPS/BudgetResourceLevel/LoadPurchasesGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<PurchaseItem>(null),
                Pagination         = new PaginationModel { TotalRecords = items.Count, PageNumber = 1, PageSize = items.Count > 0 ? items.Count : 10 },
                CurrentFilters     = filterDict
            };
        }

        // ─────────────── BUDGET BIDS CRUD ───────────────

        [HttpGet]
        public async Task<IActionResult> CreateBudgetBid(string workgroupName)
        {
            var model = new BudgetResourceCentreLevelItem { WorkgroupName = workgroupName };
            await PopulateBidDropdownsAsync(model);
            return PartialView("_AddEditBudgetResourceCentreLevel", model);
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
            var result = await _service.CreateBidAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Budget bid created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create budget bid.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditBudgetBid(string workgroupName, string account)
        {
            var result = await _service.GetBidByIdAsync(workgroupName, account);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to retrieve bid details." });

            var model = new BudgetResourceCentreLevelItem
            {
                WorkgroupName = result.Data.WorkgroupName,
                Account       = result.Data.Account,
                GenBid        = result.Data.GenBid
            };
            await PopulateBidDropdownsAsync(model);
            return PartialView("_AddEditBudgetResourceCentreLevel", model);
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
            var result = await _service.UpdateBidAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Budget bid updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update budget bid.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBudgetBid(string workgroupName, string account)
        {
            var dto = new BidDto { WorkgroupName = workgroupName, Account = account };
            var result = await _service.DeleteBidAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Budget bid deleted successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete budget bid." });
        }

        // ─────────────── PURCHASES CRUD ───────────────

        [HttpGet]
        public IActionResult CreatePurchase(string workgroupName, string account)
        {
            var model = new PurchaseItem { WorkgroupName = workgroupName, Account = account };
            return PartialView("_AddEditPurchase", model);
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
            var result = await _service.CreatePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Purchase created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create purchase.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditPurchase(string workgroupName, string account, string itemDescription)
        {
            var result = await _service.GetPurchaseByIdAsync(workgroupName, account, itemDescription);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Failed to retrieve purchase details." });

            var model = new PurchaseItem
            {
                WorkgroupName   = result.Data.WorkgroupName,
                Account         = result.Data.Account,
                ItemDescription = result.Data.ItemDescription,
                Amount          = result.Data.Amount
            };
            return PartialView("_AddEditPurchase", model);
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
            var result = await _service.UpdatePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Purchase updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update purchase.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePurchase(string workgroupName, string account, string itemDescription)
        {
            var dto = new PurchaseDto { WorkgroupName = workgroupName, Account = account, ItemDescription = itemDescription };
            var result = await _service.DeletePurchaseAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Purchase deleted successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete purchase." });
        }

        // ─────────────── HELPERS ───────────────

        private async Task PopulateProfitCentresAsync(BudgetResourceLevelViewModel model)
        {
            var result = await _service.GetProfitCentresAsync();
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
            var accountResult = await _service.GetAccountCategoriesAsync();
            model.AccountList = accountResult.Data == null ? new List<SelectListItem>() :
                accountResult.Data.Select(a => new SelectListItem
                {
                    Value    = a.AccShortName,
                    Text     = string.IsNullOrWhiteSpace(a.AccountDescription) ? a.AccShortName : $"{a.AccShortName} - {a.AccountDescription}",
                    Selected = string.Equals(model.Account, a.AccShortName, StringComparison.OrdinalIgnoreCase)
                }).ToList();
        }
    }
}
