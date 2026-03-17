using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{

    [Area("FPS")]
    [AllowAnonymous]
    public class ProgramMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;

        public ProgramMaintenanceController(IMapper mapper, IProgramService programService,
            IEmployeeService employeeService)
        {
            _mapper = mapper;
            _programService = programService;
            _employeeService = employeeService;
        }
        public async Task<IActionResult> Index()
        {
            var gridConfig = await GetProgramGridConfig();
            return View(gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadProgramGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetProgramGridConfig(queryParameters, filterDict);

            return PartialView("_DataGrid", gridConfig);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            var model = new ProgramViewModel
            {
                ProgramNo = string.Empty,
                ProgramName = string.Empty,
                Directorate = string.Empty
            };
            await PopulateDropdownsAsync(model);
            return PartialView("_AddProgram", model);
        }

        // POST: Create
        [HttpPost]        
        public async Task<IActionResult> Create([FromBody] ProgramViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_AddProgram", model);

            var dto = _mapper.Map<ProgramDto>(model);
            var response = await _programService.AddProgramAsync(dto);
            if (response.Success)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Failed to add program.");
            return PartialView("_AddProgram", model);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return NotFound();
            var model = _mapper.Map<ProgramViewModel>(response.Data);
            await PopulateDropdownsAsync(model);
            return PartialView("_EditProgram", model);
        }

        // POST: Edit
        [HttpPost]        
        public async Task<IActionResult> Edit([FromBody]ProgramViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditProgram", model);

            var dto = _mapper.Map<ProgramDto>(model);
            var response = await _programService.UpdateProgramAsync(dto);
            if (response.Success)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Failed to update program.");
            return PartialView("_EditProgram", model);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string programNo)
        {
            var response = await _programService.DeleteProgramAsync(programNo);
            if (response.Success)
                return RedirectToAction(nameof(Index));
            var viewModel = _mapper.Map<ProgramViewModel>(response.Data);
            return RedirectToAction(nameof(Index));
        }

        private async Task<DataGridConfig<ProgramViewModel>> GetProgramGridConfig(QueryParameters<string>? query = null, Dictionary<string, string>? filterDict = null)
        {
            var response = await _programService.GetAllProgramsAsync(query ?? new QueryParameters<string>());
            var programItems = new List<ProgramViewModel>();
            if (response.Data != null)
            {
                programItems = _mapper.Map<List<ProgramViewModel>>(response.Data.ToList());
            }
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(response.Pagination);

            var programGridConfig = new DataGridConfig<ProgramViewModel>
            {
                GridId = "programGrid",
                Title = "Programs",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "ProgramNo",
                AddFunction = "addProgram",
                EditFunction = "editProgram",
                DeleteFunction = "deleteProgram",
                ExtraFilterMethod = "getProgramExtraFilters",
                BindGridUrl = "/FPS/ProgramMaintenance/LoadProgramGrid",
                Data = programItems,
                Columns = GridDataProvider.GetColumnsDefination<ProgramViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return programGridConfig;
        }

        private async Task PopulateDropdownsAsync(ProgramViewModel model)
        {
            // Directorate dropdown
            var directorateResponse = await _programService.GetAllDirectoratesAsync();
            var directorates = (directorateResponse.Data ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

             model.DirectorateOptions = directorates
            .Select(d => new SelectListItem
            {
                Value = d,
                Text = d,
                Selected = string.Equals(model.Directorate, d, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

            // Manager dropdown
            var managerResponse = await _employeeService.GetAllManagersAsync();
            model.ManagerList = (managerResponse.Data ?? new List<ManagerDto>())
                .Select(m => new SelectListItem
                {
                    Value = m.Name, 
                    Text = m.Name,
                    Selected = string.Equals(model.Manager, m.Name, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }
    }
}
