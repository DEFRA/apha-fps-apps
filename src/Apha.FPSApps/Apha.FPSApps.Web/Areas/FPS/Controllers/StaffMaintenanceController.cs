using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]

    public class StaffMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;

        public StaffMaintenanceController(IMapper mapper, IEmployeeService employeeService)
        {
            _mapper = mapper;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {            
            var defaultRequest = new PaginationFilter<string> {         
                Filter = "{}"
            };
            
            var staffGridConfig = await GetEmployeeGridConfigAsync(defaultRequest, 1);

            // Create view model
            var viewModel = new StaffMaintenanceViewModel
            {
                StaffGrid = staffGridConfig                
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, int filterOption)
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

            var staffGridConfig = await GetEmployeeGridConfigAsync(request, filterOption);
            return PartialView("_DataGrid", staffGridConfig);
        }

        private async Task<DataGridConfig<EmployeeViewModel>> GetEmployeeGridConfigAsync(PaginationFilter<string> request, int filterOption)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var employeePagedData = await _employeeService.GetFilteredEmployeesAsync(queryParameters, filterOption);

            List<EmployeeViewModel> employeeItems = new List<EmployeeViewModel>();
            if (employeePagedData.Data != null)
            {
                employeeItems = _mapper.Map<List<EmployeeViewModel>>(employeePagedData.Data.ToList());
            }

            PaginationModel paginationModel = employeePagedData.Pagination == null ? new PaginationModel() : _mapper.Map<PaginationModel>(employeePagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<EmployeeViewModel>
            {
                GridId = "staffGrid",
                Title = "Staff Maintenance",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "SPNumber",
                AddFunction = "addStaff",
                EditFunction = "editStaff",
                DeleteFunction = "deleteStaff",
                ExtraFilterMethod = "getStaffExtraFilters",
                BindGridUrl = "/FPS/StaffMaintenance/LoadStaffGrid",
                Data = employeeItems,
                Columns = GridDataProvider.GetColumnsDefination<EmployeeViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditStaff");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeViewModel employeeViewModel)
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

            var employeeDto = _mapper.Map<EmployeeDto>(employeeViewModel);
            var result = await _employeeService.CreateEmployeeAsync(employeeDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Staff created successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create staff.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string spNumber)
        {
            if (string.IsNullOrWhiteSpace(spNumber))
            {
                return Json(new { success = false, message = "SP Number is required" });
            }

            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            if (result.Success)
            {
                var employeeViewModel = _mapper.Map<EmployeeViewModel>(result.Data);
                return PartialView("_AddEditStaff", employeeViewModel);
            }
            else
            {
                return Json(new { success = false, message = $"Staff with SP Number {spNumber} not found." });                
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] EmployeeViewModel employeeViewModel)
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

            var employeeDto = _mapper.Map<EmployeeDto>(employeeViewModel);
            var result = await _employeeService.UpdateEmployeeAsync(employeeDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Employee updated successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update staff.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string spNumber)
        {
            if (string.IsNullOrWhiteSpace(spNumber))
            {
                return Json(new { success = false, message = "SP Number is required" });
            }

            var result = await _employeeService.DeleteEmployeeAsync(spNumber);

            if (result.Success)
            {
                return Json(new { success = true, message = "Employee deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete staff.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployee(string spNumber)
        {
            if (string.IsNullOrWhiteSpace(spNumber))
            {
                return Json(new { success = false, message = "SP Number is required" });
            }

            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data });
            }

            return Json(new { success = false, errors = result.Errors });
        }
    }
}
