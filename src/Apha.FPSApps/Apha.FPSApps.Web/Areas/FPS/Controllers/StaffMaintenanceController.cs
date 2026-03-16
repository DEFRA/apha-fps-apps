using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    public class StaffMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;

        public StaffMaintenanceController(IMapper mapper, IEmployeeService employeeService)
        {
            _mapper = mapper;
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadEmployeeGrid(PaginationFilter<string> request)
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

            var staffGridConfig = await GetEmployeeGridConfigAsync(request);
            return PartialView("_DataGrid", staffGridConfig);
        }

        private async Task<DataGridConfig<EmployeeViewModel>> GetEmployeeGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var employeePagedData = await _employeeService.GetFilteredEmployeesAsync(queryParameters);

            List<EmployeeViewModel> employeeItems = new List<EmployeeViewModel>();
            if (employeePagedData.Data != null)
            {
                employeeItems = _mapper.Map<List<EmployeeViewModel>>(employeePagedData.Data.ToList());
            }

            PaginationModel paginationModel = _mapper.Map<PaginationModel>(employeePagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<EmployeeViewModel>
            {
                GridId = "staffGrid",
                Title = "Staff Maintenance",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "SPNumber",
                AddFunction = "addEmployee",
                EditFunction = "editEmployee",
                DeleteFunction = "deleteEmployee",
                BindGridUrl = "/FPS/StaffMaintenance/LoadEmployeeGrid",
                Data = employeeItems,
                Columns = GridDataProvider.GetColumnsDefination<EmployeeViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEmployee");
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeViewModel employeeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid employee data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeViewModel);
            var result = await _employeeService.CreateEmployeeAsync(employeeDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Employee created successfully" });
            }

            return Json(new { success = false, errors = result.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string spNumber)
        {
            if (string.IsNullOrWhiteSpace(spNumber))
            {
                return BadRequest("SP Number is required");
            }

            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            if (result.Success)
            {
                var employeeViewModel = _mapper.Map<EmployeeViewModel>(result.Data);
                return PartialView("_EditEmployee", employeeViewModel);
            }
            else
            {
                return NotFound($"Employee with SP Number {spNumber} not found.");
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
                    message = "Invalid employee data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeViewModel);
            var result = await _employeeService.UpdateEmployeeAsync(employeeDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Employee updated successfully" });
            }

            return Json(new { success = false, errors = result.Errors });
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

            return Json(new { success = false, errors = result.Errors });
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
