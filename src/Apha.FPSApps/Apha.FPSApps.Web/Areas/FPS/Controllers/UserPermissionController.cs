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
    public class UserPermissionController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userPermissionService;

        public UserPermissionController(IMapper mapper, IUserService userPermissionService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userPermissionService = userPermissionService ?? throw new ArgumentNullException(nameof(userPermissionService));
        }

        public async Task<IActionResult> Index()
        {
            var gridConfig = await GetUserGridConfigAsync();

            var optionsResponse = await _userPermissionService.GetPermissionOptionsAsync();
            var options = optionsResponse.Data ?? new PermissionOptionsDto();

            ViewBag.PermissionOptions = options;
            return View(gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadUserGrid(PaginationFilter<string> request)
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

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var gridConfig = await GetUserGridConfigAsync(queryParameters, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // GET: Create
        public IActionResult Create()
        {
            var model = new UserPermissionViewModel();
            return PartialView("_AddEditUser", model);
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserPermissionViewModel model)
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

            var dto = _mapper.Map<UserDto>(model);
            var response = await _userPermissionService.AddUserAsync(dto);
            if (response.Success)
                return Json(new { success = true, data = response.Data, message = "User created successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create user.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int userId)
        {
            var response = await _userPermissionService.GetUserByIdAsync(userId);
            if (!response.Success || response.Data == null)
                return NotFound();
            var model = _mapper.Map<UserPermissionViewModel>(response.Data);
            return PartialView("_AddEditUser", model);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UserPermissionViewModel model)
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

            var dto = _mapper.Map<UserDto>(model);
            var response = await _userPermissionService.UpdateUserAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "User updated successfully.", data = response.Data });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update user.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // DELETE
        [HttpDelete]
        public async Task<IActionResult> Delete(int userId)
        {
            var response = await _userPermissionService.DeleteUserAsync(userId);
            if (response.Success)
                return Json(new { success = true, message = "User deleted successfully.", data = response.Data });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete user.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // GET: Permissions for a user
        [HttpGet]
        public async Task<IActionResult> GetPermissions(int userId)
        {
            var response = await _userPermissionService.GetUserPermissionsAsync(userId);
            if (response.Success)
                return Json(new { success = true, data = response.Data });

            return Json(new { success = false, message = "Failed to load permissions." });
        }

        // POST: Save permissions for a user
        [HttpPost]
        public async Task<IActionResult> SavePermissions([FromBody] UserPermissionDataDto dto)
        {
            if (dto.UserId <= 0)
                return Json(new { success = false, message = "Please select a user first." });

            var response = await _userPermissionService.SaveUserPermissionsAsync(dto.UserId, dto);
            if (response.Success)
                return Json(new { success = true, message = "Permissions saved successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save permissions."
            });
        }

        private async Task<DataGridConfig<UserPermissionViewModel>> GetUserGridConfigAsync(
            QueryParameters<string>? query = null,
            Dictionary<string, string>? filterDict = null)
        {
            var response = await _userPermissionService.GetNonSuperUsersPagedAsync(query ?? new QueryParameters<string>());
            var items = new List<UserPermissionViewModel>();
            if (response.Data != null)
                items = _mapper.Map<List<UserPermissionViewModel>>(response.Data.ToList());

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            return new DataGridConfig<UserPermissionViewModel>
            {
                GridId = "userPermissionGrid",
                Title = "Users",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "UserId",
                AllowRowSelection = true,
                RowSelectFunction = "onUserRowSelect",
                AddFunction = "addUser",
                EditFunction = "editUser",
                DeleteFunction = "deleteUser",
                BindGridUrl = "/FPS/UserPermission/LoadUserGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<UserPermissionViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
