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
    public class ProgramProjectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;

        public ProgramProjectController(
            IMapper mapper,
            IProjectService projectService,
            IProgramService programService,
            IEmployeeService employeeService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _programService = programService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index(string? programNo = null)
        {
            var programmeList = await GetProgrammeListAsync();
            var isValidProgramNo = !string.IsNullOrWhiteSpace(programNo)
                && programmeList.Any(p => p.Value == programNo);
            var selectedProgramNo = isValidProgramNo
                ? programNo ?? string.Empty
                : programmeList.FirstOrDefault()?.Value ?? string.Empty;

            var projectsGrid = new DataGridConfig<ProgramProjectItem>
            {
                GridId = "programProjectGrid",
                Title = "Projects within Programme",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true,
                AllowRowSelection = true,
                RowSelectFunction = "selectProgramProject",
                EditFunction = "editProgramProject",
                DeleteFunction = "deleteProgramProject",
                ExtraFilterMethod = "getProgramProjectExtraFilters",
                BindGridUrl = "/FPS/ProgramProject/LoadProgramProjectGrid",
                Data = new List<ProgramProjectItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ProgramProjectItem>(),
                Pagination = new PaginationModel()
            };

            var model = new ProgramProjectViewModel
            {
                SelectedProgramNo = selectedProgramNo,
                ProgrammeList = programmeList,
                ProjectsGrid = projectsGrid
            };

            return View(model);
        }

        /// <summary>
        /// Loads the read-only project selector grid used by ProgramStaffPlan, ProgramAnimalPlan, etc.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectGrid(
            PaginationFilter<string> request, string? programNo = null)
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
            var projectsData = await _projectService.GetProjectsByProgramAsync(
                queryParameters, programNo ?? string.Empty);

            var projectItems = new List<ProjectViewModel>();
            if (projectsData.Success && projectsData.Data != null)
            {
                projectItems = _mapper.Map<List<ProjectViewModel>>(projectsData.Data);
            }

            var paginationModel = _mapper.Map<PaginationModel>(projectsData.Pagination)
                ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<ProjectViewModel>
            {
                GridId = "projectGrid",
                Title = "Projects",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectProject",
                ExtraFilterMethod = "getProjectExtraFilters",
                BindGridUrl = "/FPS/ProgramProject/LoadProjectGrid",
                Data = projectItems,
                Columns = GridDataProvider.GetColumnsDefination<ProjectViewModel>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Loads the full editable projects DataGrid for the Plan Projects Individually page.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProgramProjectGrid(
            PaginationFilter<string> request, string? programNo = null)
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
            var projectsData = await _projectService.GetProjectsByProgramAsync(
                queryParameters, programNo ?? string.Empty);

            var projectItems = new List<ProgramProjectItem>();
            if (projectsData.Success && projectsData.Data != null)
            {
                projectItems = _mapper.Map<List<ProgramProjectItem>>(projectsData.Data);
            }

            var paginationModel = _mapper.Map<PaginationModel>(projectsData.Pagination)
                ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<ProgramProjectItem>
            {
                GridId = "programProjectGrid",
                Title = "Projects within Programme",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true,
                AllowRowSelection = true,
                RowSelectFunction = "selectProgramProject",
                EditFunction = "editProgramProject",
                DeleteFunction = "deleteProgramProject",
                ExtraFilterMethod = "getProgramProjectExtraFilters",
                BindGridUrl = "/FPS/ProgramProject/LoadProgramProjectGrid",
                Data = projectItems,
                Columns = GridDataProvider.GetColumnsDefination<ProgramProjectItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// GET: returns programme info (name) as JSON for client-side updates.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProgramInfo(string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
                return Json(new { success = false, message = "Programme number is required." });

            var result = await _programService.GetProgramByIdAsync(programNo);
            if (result.Success && result.Data != null)
            {
                return Json(new
                {
                    success = true,
                    programmeName = result.Data.ProgramName
                });
            }

            return Json(new { success = false, message = "Programme not found." });
        }

        /// <summary>
        /// GET: returns summed financial totals across all projects in a programme.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjectTotals(string? programNo)
        {
            var zero = new { budgetCvl = 0M, budgetExt = 0M, transferIncome = 0M, planCaseWorkDebit = 0M };

            if (string.IsNullOrWhiteSpace(programNo))
                return Json(zero);

            var query = new QueryParameters<string> { Page = 1, PageSize = 9999 };
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            if (!result.Success || result.Data == null)
                return Json(zero);

            return Json(new
            {
                budgetCvl        = result.Data.Sum(p => p.BudgetCvl ?? 0M),
                budgetExt        = result.Data.Sum(p => p.BudgetExt ?? 0M),
                transferIncome   = result.Data.Sum(p => (decimal)p.TransferIncome),
                planCaseWorkDebit = result.Data.Sum(p => p.PlanCaseWorkDebit ?? 0M)
            });
        }

        /// <summary>
        /// GET: returns the edit partial for a project within a programme.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                return BadRequest();

            var result = await _projectService.GetProjectByIdAsync(parentProject);
            if (!result.Success || result.Data == null)
                return NotFound();

            var model = _mapper.Map<ProgramProjectEditViewModel>(result.Data);
            await PopulateDropdownsAsync(model);
            return PartialView("_AddEditProgramProject", model);
        }

        /// <summary>
        /// POST: updates a project's details from the programme projects grid.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProgramProjectEditViewModel model)
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

            var dto = _mapper.Map<ProjectDto>(model);
            var result = await _projectService.UpdateProjectAsync(dto);

            if (result.Success)
                return Json(new { success = true, data = result.Data, message = "Project updated successfully." });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update project.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// DELETE: removes a project from the programme.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                return Json(new { success = false, message = "Project ID is required." });

            var result = await _projectService.DeleteProjectAsync(parentProject);

            if (result.Success && result.Data)
                return Json(new { success = true, message = "Project deleted successfully.", data = result.Data });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete project.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task PopulateDropdownsAsync(ProgramProjectEditViewModel model)
        {
            var managersResult = await _employeeService.GetAllManagersAsync();
            model.ManagerList = managersResult.Success && managersResult.Data != null
                ? managersResult.Data
                    .Select(m => new SelectListItem { Value = m.Name, Text = m.Name })
                    .ToList()
                : new List<SelectListItem>();

            var programsResult = await _programService.GetAllProgramsAsync();
            model.ProgramList = programsResult.Success && programsResult.Data != null
                ? programsResult.Data
                    .Select(p => new SelectListItem { Value = p.ProgramNo, Text = $"{p.ProgramNo} - {p.ProgramName}" })
                    .ToList()
                : new List<SelectListItem>();

            var customersResult = await _projectService.GetAllCustomersAsync();
            model.CustomerList = customersResult.Success && customersResult.Data != null
                ? customersResult.Data
                    .Select(c => new SelectListItem { Value = c.Customer, Text = c.Customer })
                    .ToList()
                : new List<SelectListItem>();

            var projectGroupsResult = await _projectService.GetAllProjectGroupsAsync();
            model.ProjectGroupList = projectGroupsResult.Success && projectGroupsResult.Data != null
                ? projectGroupsResult.Data
                    .Select(g => new SelectListItem { Value = g.ProjectGroupName, Text = g.ProjectGroupName })
                    .ToList()
                : new List<SelectListItem>();

            var contractsResult = await _projectService.GetAllContractsAsync();
            model.ContractList = contractsResult.Success && contractsResult.Data != null
                ? contractsResult.Data
                    .Select(c => new SelectListItem { Value = c.ContractNo, Text = c.ContractNo })
                    .ToList()
                : new List<SelectListItem>();

            var diseasesResult = await _projectService.GetAllDiseasesAsync();
            model.DiseaseList = diseasesResult.Success && diseasesResult.Data != null
                ? diseasesResult.Data
                    .Select(d => new SelectListItem { Value = d.Disease, Text = d.Disease })
                    .ToList()
                : new List<SelectListItem>();

            var statusesResult = await _projectService.GetAllStatusesAsync();
            model.StatusList = statusesResult.Success && statusesResult.Data != null
                ? statusesResult.Data
                    .Select(s => new SelectListItem { Value = s.Status, Text = s.Status })
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetProgrammeListAsync()
        {
            var result = await _programService.GetAllProgramsAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProgramNo))
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProgramNo!,
                        Text  = $"{p.ProgramNo} - {p.ProgramName}"
                    })
                    .ToList();
            }
            return new List<SelectListItem>();
        }
    }
}

