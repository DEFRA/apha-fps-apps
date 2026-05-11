using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope,FPSApiSettings:Scope")]
    public class ProjectProfileController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectMonthService _projectMonthService;
        private readonly IProjectProfileService _projectProfileService;
        private readonly IProjectService _projectService;

        public ProjectProfileController(
            IMapper mapper,
            IProjectMonthService projectMonthService,
            IProjectProfileService projectProfileService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _projectMonthService = projectMonthService;
            _projectProfileService = projectProfileService;
            _projectService = projectService;
        }

        /// <summary>
        /// Renders the Project Profile index view, loading project details and dropdown list.
        /// </summary>
        /// <param name="parentProject">Optional project code to pre-select and load details for.</param>
        public async Task<IActionResult> Index(string? parentProject)
        {
            var projectsTask = BuildProjectsListAsync(parentProject);
            var projectTask  = string.IsNullOrWhiteSpace(parentProject)
                ? Task.FromResult<ProjectDto?>(null)
                : FetchProjectDetailsAsync(parentProject);

            await Task.WhenAll(projectsTask, projectTask);

            return View(new ProjectProfileViewModel
            {
                ParentProject = parentProject ?? string.Empty,
                Projects      = projectsTask.Result,
                ProjectTitle  = projectTask.Result?.ProjectTitle ?? string.Empty,
                BudgetCvl     = projectTask.Result?.BudgetCvl,
            });
        }

        /// <summary>
        /// Loads the cost profile data grid as a partial view for a given project.
        /// </summary>
        /// <param name="request">Pagination and filter parameters for the grid.</param>
        /// <param name="parentProject">Optional project code to filter the cost profile data.</param>
        [HttpPost]
        public async Task<IActionResult> LoadCostProfileGrid(PaginationFilter<string> request, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gridConfig = await BuildCostProfileGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── GRAPH DATA (JSON) ─────────────────────────────────────────────────

        /// <summary>
        /// Returns project title and budget details as JSON for the given project code.
        /// </summary>
        /// <param name="parentProject">The project code to retrieve details for.</param>
        [HttpGet]
        [ActionName("GetProjectDetailsAsync")]
        public async Task<IActionResult> GetProjectDetailsAsync(string parentProject)
        {
            var result = await FetchProjectDetailsAsync(parentProject);
            if (result == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                projectTitle = result.ProjectTitle,
                budgetCvl = result.BudgetCvl
            });
        }

        /// <summary>
        /// Returns the total sum of cost profile values across all months for a given project.
        /// </summary>
        /// <param name="parentProject">The project code to calculate the total cost profile for.</param>
        [HttpGet]
        public async Task<IActionResult> GetTotalCostProfile(string parentProject)
        {
            var response = await _projectMonthService.GetProjectMonthByProjectAsync(parentProject);
            if (!response.Success)
                return Json(new { success = false });

            var total = (response.Data ?? new List<ProjectMonthDto>()).Sum(m => m.CostProfile ?? 0m);
            return Json(new { success = true, data = total });
        }

        /// <summary>
        /// Returns monthly profile and cost data as JSON for rendering the non-cumulative graph.
        /// </summary>
        /// <param name="parentProject">The project code to retrieve profile data for.</param>
        [HttpGet]
        public async Task<IActionResult> GetProfileData(string parentProject)
        {
            var result = await _projectProfileService.GetProfileDataAsync(parentProject);
            if (!result.Success)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = result.Data?.Select(d => new
                {
                    monthNo = d.MonthNo,
                    profile = d.Profile,
                    totalCost = d.TotalCost
                })
            });
        }

        /// <summary>
        /// Returns cumulative profile and cost data as JSON for rendering the cumulative graph.
        /// </summary>
        /// <param name="parentProject">The project code to retrieve cumulative data for.</param>
        [HttpGet]
        public async Task<IActionResult> GetCumulativeData(string parentProject)
        {
            var result = await _projectProfileService.GetCumulativeDataAsync(parentProject);
            if (!result.Success)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = result.Data?.Select(d => new
                {
                    monthNo = d.MonthNo,
                    cumulativeProfile = d.CumulativeProfile,
                    cumulativeCost = d.CumulativeCost
                })
            });
        }

        // ── COST PROFILE CRUD ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the add/edit partial view for a cost profile month record.
        /// Passing <c>monthNo = 0</c> opens the form in add mode; any other value loads the existing record for editing.
        /// </summary>
        /// <param name="project">The project code the month record belongs to.</param>
        /// <param name="monthNo">The month number to edit, or <c>0</c> to create a new record.</param>
        [HttpGet]
        public async Task<IActionResult> GetProjectMonth(string project, int monthNo)
        {
            if (monthNo == 0)
            {
                return PartialView("_AddEditProjectMonth", new ProjectMonthItem
                {
                    Project = project
                });
            }

            var result = await _projectMonthService.GetProjectMonthAsync(project, monthNo);
            if (!result.Success || result.Data == null) return NotFound();
            return PartialView("_AddEditProjectMonth", _mapper.Map<ProjectMonthItem>(result.Data));
        }

        /// <summary>
        /// Creates or updates a cost profile month record. A <c>MonthNo</c> of <c>0</c> triggers a create; otherwise an update is performed.
        /// </summary>
        /// <param name="model">The cost profile month data submitted from the form.</param>
        [HttpPost]
        public async Task<IActionResult> SaveProjectMonth([FromBody] ProjectMonthItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<ProjectMonthDto>(model);
            var isNew = model.MonthNo == 0;

            var result = isNew
                ? await _projectMonthService.CreateProjectMonthAsync(dto)
                : await _projectMonthService.UpdateProjectMonthAsync(dto);

            var successMsg = isNew ? "Cost profile month saved successfully." : "Cost profile month updated successfully.";

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to save cost profile month.",
                errors = (result.Errors ?? []).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes the specified cost profile month record for a project.
        /// </summary>
        /// <param name="project">The project code the month record belongs to.</param>
        /// <param name="monthNo">The month number of the record to delete.</param>
        [HttpDelete]
        public async Task<IActionResult> DeleteProjectMonth(string project, int monthNo)
        {
            var result = await _projectMonthService.DeleteProjectMonthAsync(project, monthNo);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete cost profile month." });
        }

        /// <summary>
        /// Builds and returns the data grid configuration for the cost profile grid.
        /// </summary>
        /// <param name="request">Pagination and filter parameters.</param>
        /// <param name="parentProject">The project code to load cost profile rows for.</param>
        private async Task<DataGridConfig<ProjectMonthItem>> BuildCostProfileGridAsync(
            PaginationFilter<string> request, string? parentProject)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var response = await _projectMonthService.GetProjectMonthByProjectAsync(parentProject ?? string.Empty);

            var items = response.Data != null
                ? _mapper.Map<List<ProjectMonthItem>>(response.Data)
                : new List<ProjectMonthItem>();

            return new DataGridConfig<ProjectMonthItem>
            {
                GridId = "costProfileGrid",
                Title = "",
                KeyProperty = "MonthNo",
                AllowAdd= false,
                AllowEdit = true,
                AllowDelete= false,
                ShowPagination=false,
                AddFunction = "addProjectMonth",
                EditFunction = "editProjectMonth",
                DeleteFunction = "deleteProjectMonth",
                BindGridUrl = string.IsNullOrEmpty(parentProject)
                    ? "/PACT/ProjectProfile/LoadCostProfileGrid"
                    : $"/PACT/ProjectProfile/LoadCostProfileGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectMonthItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Fetches project details for the given project code, returning <c>null</c> if not found.
        /// </summary>
        /// <param name="parentProject">The project code to look up.</param>
        private async Task<ProjectDto?> FetchProjectDetailsAsync(string parentProject)
        {
            var result = await _projectService.GetProjectByIdAsync(parentProject);
            return result.Success ? result.Data : null;
        }

        private async Task<List<SelectListItem>> BuildProjectsListAsync(string? selectedProject = null)
        {
            var result = await _projectService.GetAllPactProjectsAsync();
            return (result.Success && result.Data != null)
                ? result.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value    = p.ParentProject,
                        Text     = p.ParentProject,
                        Selected = p.ParentProject == selectedProject
                    })
                    .ToList()
                : [];
        }
    }
}