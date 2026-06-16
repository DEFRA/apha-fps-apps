using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class AccountCategoryMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAccountCategoryService _accountCategoryService;

        public AccountCategoryMaintenanceController(IMapper mapper, IAccountCategoryService accountCategoryService)
        {
            _mapper = mapper;
            _accountCategoryService = accountCategoryService;
        }

        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}"
            };

            var accountCategoryGridConfig = await GetAccountCategoryGridConfigAsync(defaultRequest, "all");

            var viewModel = new AccountCategoryMaintenanceViewModel
            {
                AccountCategoryGrid = accountCategoryGridConfig
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAccountCategoryGrid(PaginationFilter<string> request, string filterType = "all")
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var accountCategoryGridConfig = await GetAccountCategoryGridConfigAsync(request, filterType);
            return PartialView("_DataGrid", accountCategoryGridConfig);
        }

        private async Task<DataGridConfig<AccountCategoryViewModel>> GetAccountCategoryGridConfigAsync(PaginationFilter<string> request, string filterType)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var accountCategoryPagedData = await _accountCategoryService.GetFilteredAccountCategoriesAsync(queryParameters, filterType);

            List<AccountCategoryViewModel> accountCategoryItems = new List<AccountCategoryViewModel>();
            if (accountCategoryPagedData.Data != null)
            {
                accountCategoryItems = _mapper.Map<List<AccountCategoryViewModel>>(accountCategoryPagedData.Data.ToList());
            }

            PaginationModel paginationModel = accountCategoryPagedData.Pagination == null ? new PaginationModel() : _mapper.Map<PaginationModel>(accountCategoryPagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<AccountCategoryViewModel>
            {
                GridId = "accountCategoryGrid",
                Title = "Account Category Maintenance",
                KeyProperty = "AccShortName",
                AddFunction = "addAccountCategory",
                EditFunction = "editAccountCategory",
                DeleteFunction = "deleteAccountCategory",
                ExtraFilterMethod = "getAccountCategoryExtraFilters",
                BindGridUrl = "/FPS/AccountCategoryMaintenance/LoadAccountCategoryGrid",
                Data = accountCategoryItems,
                Columns = GridDataProvider.GetColumnsDefination<AccountCategoryViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditAccountCategory");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccountCategoryViewModel accountCategoryViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var accountCategoryDto = _mapper.Map<AccountCategoryDto>(accountCategoryViewModel);
            var result = await _accountCategoryService.CreateAccountCategoryAsync(accountCategoryDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Account category created successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create account category.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string accShortName)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
            {
                return Json(new { success = false, message = "Account Short Name is required" });
            }

            var result = await _accountCategoryService.GetAccountCategoryByIdAsync(accShortName);

            if (result.Success)
            {
                var accountCategoryViewModel = _mapper.Map<AccountCategoryViewModel>(result.Data);
                return PartialView("_AddEditAccountCategory", accountCategoryViewModel);
            }
            else
            {
                return Json(new { success = false, message = $"Account category with Short Name {accShortName} not found." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] AccountCategoryViewModel accountCategoryViewModel, [FromQuery] string? originalAccShortName = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            // Use originalAccShortName if provided (when AccShortName is being changed), otherwise use current name
            var identifyingAccShortName = !string.IsNullOrWhiteSpace(originalAccShortName) ? originalAccShortName : accountCategoryViewModel.AccShortName;

            var accountCategoryDto = _mapper.Map<AccountCategoryDto>(accountCategoryViewModel);
            var result = await _accountCategoryService.UpdateAccountCategoryAsync(identifyingAccShortName, accountCategoryDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Account category updated successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update account category.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string accShortName)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
            {
                return Json(new { success = false, message = "Account Short Name is required" });
            }

            var result = await _accountCategoryService.DeleteAccountCategoryAsync(accShortName);

            if (result.Success)
            {
                return Json(new { success = true, message = "Account category deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete account category.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountCategory(string accShortName)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
            {
                return Json(new { success = false, message = "Account Short Name is required" });
            }

            var result = await _accountCategoryService.GetAccountCategoryByIdAsync(accShortName);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, errors = result.Errors });
        }
    }
}
