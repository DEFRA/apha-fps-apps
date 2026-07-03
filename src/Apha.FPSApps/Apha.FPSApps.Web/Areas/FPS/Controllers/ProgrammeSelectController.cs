using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Application.Interfaces;
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
    public class ProgrammeSelectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;

        public ProgrammeSelectController(
            IMapper mapper,
            IProgramService programService,
            IProjectService projectService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _programService = programService;
            _projectService = projectService;
            _appStateService = appStateService;
        }

        /// <summary>
        /// Displays the Programme Manager - a read-only project selection interface
        /// </summary>
        public async Task<IActionResult> Index(string? programNo = null, string? projectSearch = null)
        {
            var programmeList = await GetProgrammeListAsync();

            // Only use programNo if it is explicitly provided and valid — never fall back to session or first item
            var isValidProgramNo = !string.IsNullOrWhiteSpace(programNo)
                && programmeList.Any(p => p.Value == programNo);
            var selectedProgramNo = isValidProgramNo ? programNo! : string.Empty;

            // Save to session only when user has made an explicit selection
            if (isValidProgramNo)
                await _appStateService.SetSessionAsync(SessionKeys.SelectedProgramNo, selectedProgramNo);

            var defaultRequest = new PaginationFilter<string>();
            var grid = await BuildProjectsGridAsync(defaultRequest, selectedProgramNo, projectSearch);

            var model = new ProgrammeSelectViewModel
            {
                SelectedProgramNo = selectedProgramNo,
                ProjectSearch = projectSearch ?? string.Empty,
                ProgrammeList = programmeList,
                ProjectsGrid = grid
            };

            return View(model);
        }

        /// <summary>
        /// Saves the selected programme number to session (called client-side via AJAX).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveProgrammeSession([FromBody] string programNo)
        {
            await _appStateService.SetSessionAsync(SessionKeys.SelectedProgramNo, programNo);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LoadProjectsGrid(PaginationFilter<string> request, string programNo, string? projectSearch = null)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(programNo))
                return BadRequest(ModelState);

            // projectSearch may arrive as a standalone param or inside request.Filter as JSON
            if (string.IsNullOrWhiteSpace(projectSearch) && !string.IsNullOrWhiteSpace(request.Filter))
            {
                var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter);
                filterDict?.TryGetValue("projectSearch", out projectSearch);
            }

            var gridConfig = await BuildProjectsGridAsync(request, programNo, projectSearch);

            return PartialView("_DataGrid", gridConfig);
        }

        #region Private Helpers

        private async Task<List<SelectListItem>> GetProgrammeListAsync()
        {
            var response = await _programService.GetAllProgramsAsync();

            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .OrderBy(p => p.ProgramNo)
                .Select(p => new SelectListItem
                {
                    Value = p.ProgramNo,
                    Text = p.ProgramNo
                })
                .ToList();
        }

        private async Task<DataGridConfig<ProgrammeSelectProjectItem>> BuildProjectsGridAsync(
            PaginationFilter<string> request, string programNo, string? projectSearch = null)
        {
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // Pass projectSearch as a server-side filter so the API filters before paging
            if (!string.IsNullOrWhiteSpace(projectSearch))
                queryParameters.Filter = JsonConvert.SerializeObject(new { ParentProject = projectSearch });

            var response = !string.IsNullOrWhiteSpace(programNo)
                ? await _projectService.GetProjectsByProgramAsync(queryParameters, programNo)
                : null;

            var items = response?.Data != null
                ? response.Data.Select(p => new ProgrammeSelectProjectItem
                  {
                      Program = p.Program ?? string.Empty,
                      ParentProject = p.ParentProject ?? string.Empty
                  }).ToList()
                : new List<ProgrammeSelectProjectItem>();

            var pagination = response?.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProgrammeSelectProjectItem>
            {
                GridId = "projectsGrid",
                Title = "Projects",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = false,
                AllowView = true,
                EditFunction = "editProject",
                ViewFunction = "planProject",
                ExtraFilterMethod = "getProjectsExtraFilters",
                BindGridUrl = $"/FPS/ProgrammeSelect/LoadProjectsGrid?programNo={programNo}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProgrammeSelectProjectItem>(),
                Pagination = pagination
            };
        }

        #endregion
    }
}
