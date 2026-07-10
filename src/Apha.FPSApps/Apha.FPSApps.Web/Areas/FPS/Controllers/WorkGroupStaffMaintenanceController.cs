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
        private readonly IEmployeeService _employeeService;
        private readonly IWorkGroupGradeService _workGroupGradeService;

        public WorkGroupStaffMaintenanceController(
            IMapper mapper,
            IWorkGroupEmployeeService workGroupEmployeeService,
            IEmployeeService employeeService,
            IWorkGroupGradeService workGroupGradeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _workGroupEmployeeService = workGroupEmployeeService ?? throw new ArgumentNullException(nameof(workGroupEmployeeService));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _workGroupGradeService = workGroupGradeService ?? throw new ArgumentNullException(nameof(workGroupGradeService));
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

            var uiFilterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var apiFilterDict = uiFilterDict != null
                ? new Dictionary<string, string>(uiFilterDict, StringComparer.OrdinalIgnoreCase)
                : null;

            if (apiFilterDict != null)
            {
                request.Filter = JsonConvert.SerializeObject(apiFilterDict);
            }

            var uiSortBy = request.SortBy;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var gridConfig = await GetWorkGroupStaffGridConfigAsync(queryParameters, uiFilterDict);
            if (gridConfig.Pagination != null)
            {
                gridConfig.Pagination.SortColumn = uiSortBy;
            }

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new WorkGroupEmployeeStaffItem
            {
                PactId = string.Empty,
                Name = string.Empty,
                WorkGroupGrade = string.Empty,
                PersonStatus = string.Empty,
                HrsPaid = 0,
                Leave = 0,
                SickSpecial = 0,
                HrsAvail = 0
            };

            ViewData["IsEditMode"] = false;
            await PopulateLookupDataAsync(model);
            return PartialView("~/Areas/FPS/Views/WorkGroupStaffMaintenance/_AddEditWorkGroupStaff.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkGroupEmployeeStaffDto model)
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

            var response = await _workGroupEmployeeService.CreateWorkGroupEmployeeForStaffAsync(model);
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
            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(pactId);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<WorkGroupEmployeeStaffItem>(response.Data);
            ViewData["IsEditMode"] = true;
            await PopulateLookupDataAsync(model);
            return PartialView("~/Areas/FPS/Views/WorkGroupStaffMaintenance/_AddEditWorkGroupStaff.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] WorkGroupEmployeeStaffDto model)
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

            var response = await _workGroupEmployeeService.UpdateWorkGroupEmployeeForStaffAsync(model);
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

        private async Task<DataGridConfig<WorkGroupEmployeeStaffItem>> GetWorkGroupStaffGridConfigAsync(
            QueryParameters<string>? query = null,
            Dictionary<string, string>? filterDict = null)
        {
            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync(query ?? new QueryParameters<string>(), string.Empty);

            var items = new List<WorkGroupEmployeeStaffItem>();
            if (response.Data != null)
            {
                items = _mapper.Map<List<WorkGroupEmployeeStaffItem>>(response.Data.ToList());
            }

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            return new DataGridConfig<WorkGroupEmployeeStaffItem>
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
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupEmployeeStaffItem>(null),
                ShowCheckboxColumn = false,
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task PopulateLookupDataAsync(WorkGroupEmployeeStaffItem model)
        {
            var lookupQuery = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 5000
            };

            var employeeResponse = await _employeeService.GetFilteredEmployeesAsync(lookupQuery, 0);
            var employees = employeeResponse.Success && employeeResponse.Data != null
                ? employeeResponse.Data
                : [];

            model.StaffLookupOptions = employees
                .Select(e => new WorkGroupStaffLookupItem
                {
                    PactId = string.Empty,
                    Name = ResolveEmployeeName(e),
                    SpNumber = ResolveEmployeeSpNumber(e)
                })
                .Where(s => !string.IsNullOrWhiteSpace(s.Name) && !string.IsNullOrWhiteSpace(s.SpNumber))
                .GroupBy(s => s.SpNumber, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(s => s.Name)
                .ToList();

            var workGroupGradeQuery = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 5000
            };

            var workGroupGradeResponse = await _workGroupGradeService.GetAllWorkgroupGradesPagedAsync(workGroupGradeQuery);
            var workGroupGradeSourceData = workGroupGradeResponse.Success && workGroupGradeResponse.Data != null
                ? workGroupGradeResponse.Data
                : [];

            model.WgGradeOptions = workGroupGradeSourceData
                .Select(g => g.WgGrade)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g)
                .ToList();

            if (!string.IsNullOrWhiteSpace(model.Name) && !string.IsNullOrWhiteSpace(model.SpNumber)
                && !model.StaffLookupOptions.Any(s => string.Equals(s.SpNumber, model.SpNumber, StringComparison.OrdinalIgnoreCase)))
            {
                model.StaffLookupOptions.Add(new WorkGroupStaffLookupItem
                {
                    PactId = model.PactId ?? string.Empty,
                    Name = model.Name,
                    SpNumber = model.SpNumber
                });

                model.StaffLookupOptions = model.StaffLookupOptions
                    .OrderBy(s => s.Name)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.WorkGroupGrade) && !model.WgGradeOptions.Any(g => string.Equals(g, model.WorkGroupGrade, StringComparison.OrdinalIgnoreCase)))
            {
                model.WgGradeOptions.Add(model.WorkGroupGrade);
                model.WgGradeOptions = model.WgGradeOptions
                    .OrderBy(g => g)
                    .ToList();
            }
        }

        private static string ResolveEmployeeName(EmployeeDto employee)
        {
            var directName = GetEmployeePropertyValue(employee, "Name", "FullName");
            if (!string.IsNullOrWhiteSpace(directName))
                return directName;

            var firstName = GetEmployeePropertyValue(employee, "FirstName");
            var lastName = GetEmployeePropertyValue(employee, "LastName");
            return $"{firstName} {lastName}".Trim();
        }

        private static string ResolveEmployeeSpNumber(EmployeeDto employee)
        {
            return GetEmployeePropertyValue(employee, "SPNumber", "SpNumber", "EmployeeNumber", "StaffNumber");
        }

        private static string GetEmployeePropertyValue(EmployeeDto employee, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var prop = employee.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                var value = prop?.GetValue(employee)?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }
    }
}
