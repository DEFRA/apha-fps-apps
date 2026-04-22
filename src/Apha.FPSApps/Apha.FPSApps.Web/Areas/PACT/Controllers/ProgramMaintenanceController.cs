using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using FpsDto = Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProgramMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;

        public ProgramMaintenanceController(
            IMapper mapper,
            IProgramService programService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _programService = programService;
            _projectService = projectService;
        }

        // ── INDEX ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? programNo = null)
        {
            var programList = await GetProgramListAsync();

            var isValid = !string.IsNullOrWhiteSpace(programNo)
                          && programList.Any(p => p.Value == programNo);
            var selectedProgramNo = isValid
                ? programNo!
                : programList.FirstOrDefault()?.Value ?? string.Empty;

            var defaultRequest = new PaginationFilter<string>();
            var grid = await BuildEmptyProjectsGrid(defaultRequest, string.IsNullOrEmpty(programNo) ? selectedProgramNo : programNo);


            var model = new PactProgramMaintenanceViewModel
            {
                SelectedProgramNo = selectedProgramNo,
                ProgramList = programList,
                ProjectsGrid = grid
            };

            return View(model);
        }

        // ── LOAD PROGRAM (AJAX) ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetProgram(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return Json(new { success = false, message = "Program not found." });

            var vm = _mapper.Map<PactProgramViewModel>(response.Data);
            return Json(new { success = true, data = vm });
        }

        // ── SAVE PROGRAM (AJAX) ─────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] PactProgramViewModel model)
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

            var dto = _mapper.Map<FpsDto.ProgramDto>(model);
            var response = await _programService.UpdateProgramAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Program saved successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save program.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── LOAD PROJECTS GRID (AJAX) ────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProjectsGrid(PaginationFilter<string> request, string programNo)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(programNo))
                return BadRequest(ModelState);

            var gridConfig = await BuildEmptyProjectsGrid(request, programNo);
           
            return PartialView("_DataGrid", gridConfig);
        }

        // ── PRIVATE HELPERS ──────────────────────────────────────────────────

        private async Task<List<SelectListItem>> GetProgramListAsync()
        {
            var response = await _programService.GetAllProgramsAsync();
            if (!response.Success || response.Data == null)
                return [];

            return response.Data
                .Select(p => new SelectListItem
                {
                    Value = p.ProgramNo,
                    Text = $"{p.ProgramNo} - {p.ProgramName}"
                })
                .ToList();
        }

        private async Task<DataGridConfig<PactProgramProjectItem>> BuildEmptyProjectsGrid(
            PaginationFilter<string> request, string programNo)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _projectService.GetProjectsByProgramAsync(query, programNo);

            var items = response.Data != null
                ? _mapper.Map<List<PactProgramProjectItem>>(response.Data)
                : [];

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            //var gridConfig = new DataGridConfig<PactProgramProjectItem>
            return new DataGridConfig<PactProgramProjectItem>
            {
                GridId = "projectsGrid",
                Title = "Projects",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectProject",
                ExtraFilterMethod = "getProjectsGridExtraFilters",
                BindGridUrl = "/PACT/ProgramMaintenance/LoadProjectsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PactProgramProjectItem>(),
                Pagination = pagination
            };
        }
    }
}
