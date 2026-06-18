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
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WorkGroupStaffMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupEmployeeService _workGroupEmployeeService;

        public WorkGroupStaffMaintenanceController(IMapper mapper, IWorkGroupEmployeeService workGroupEmployeeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _workGroupEmployeeService = workGroupEmployeeService ?? throw new ArgumentNullException(nameof(workGroupEmployeeService));
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new WorkGroupStaffMaintenanceViewModel
            {
                WGStaffGrid = await GetWorkGroupStaffGridConfigAsync()
            };

            return View("~/Areas/FPS/Views/WorkGroupStaffMaintenance/Index.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadWGStaffGrid(PaginationFilter<string> request)
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
            var gridConfig = await GetWorkGroupStaffGridConfigAsync(queryParameters, filterDict);

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new WorkGroupEmployeeItem
            {
                PactId = string.Empty,
                StaffName = string.Empty,
                WgGrade = string.Empty,
                PersonStatus = string.Empty
            };

            return PartialView("~/Areas/FPS/Views/WorkGroupStaffMaintenance/_AddEditWorkGroupStaff.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkGroupEmployeeDto model)
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

            var response = await _workGroupEmployeeService.CreateWorkGroupEmployeeAsync(model);
            if (response.Success)
            {
                return Json(new { success = true, data = response.Data, message = "WG Staff record created successfully" });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create WG Staff record.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string pactId)
        {
            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeByIdAsync(pactId);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<WorkGroupEmployeeItem>(response.Data);
            return PartialView("~/Areas/FPS/Views/WorkGroupStaffMaintenance/_AddEditWorkGroupStaff.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] WorkGroupEmployeeDto model)
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

            var response = await _workGroupEmployeeService.UpdateWorkGroupEmployeeAsync(model);
            if (response.Success)
            {
                return Json(new
                {
                    success = true,
                    message = "WG Staff record updated successfully.",
                    data = response.Data
                });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update WG Staff record.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string pactId)
        {
            var response = await _workGroupEmployeeService.DeleteWorkGroupEmployeeAsync(pactId);
            if (response.Success)
            {
                return Json(new
                {
                    success = true,
                    message = "WG Staff record deleted successfully.",
                    data = response.Data
                });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete WG Staff record.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task<DataGridConfig<WorkGroupEmployeeItem>> GetWorkGroupStaffGridConfigAsync(
            QueryParameters<string>? query = null,
            Dictionary<string, string>? filterDict = null)
        {
            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeAsync(query ?? new QueryParameters<string>(), string.Empty);

            var items = new List<WorkGroupEmployeeItem>();
            if (response.Data != null)
            {
                items = _mapper.Map<List<WorkGroupEmployeeItem>>(response.Data.ToList());
            }

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            return new DataGridConfig<WorkGroupEmployeeItem>
            {
                GridId = "wgStaffGrid",
                Title = "WG Staff",
                ShowPagination = true,
                KeyProperty = "PactId",
                AddFunction = "addMaintWGStaff",
                EditFunction = "editMaintWGStaff",
                DeleteFunction = "deleteMaintWGStaff",
                ExtraFilterMethod = "getMaintWGStaffExtraFilters",
                BindGridUrl = "/FPS/WorkGroupStaffMaintenance/LoadWGStaffGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupEmployeeItem>(null),
                ShowCheckboxColumn = false,
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
