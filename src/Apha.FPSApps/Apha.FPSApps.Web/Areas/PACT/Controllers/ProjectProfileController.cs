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

        [HttpPost]
        public async Task<IActionResult> LoadCostProfileGrid(PaginationFilter<string> request, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gridConfig = await BuildCostProfileGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── GRAPH DATA (JSON) ─────────────────────────────────────────────────

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

        [HttpGet]
        public async Task<IActionResult> GetTotalCostProfile(string parentProject)
        {
            var response = await _projectMonthService.GetProjectMonthByProjectAsync(parentProject);
            if (!response.Success)
                return Json(new { success = false });

            var total = (response.Data ?? new List<ProjectMonthDto>()).Sum(m => m.CostProfile ?? 0m);
            return Json(new { success = true, data = total });
        }

        [HttpGet]
        public async Task<IActionResult> GetProfileGraphData(string parentProject)
        {
            var result = await _projectProfileService.GetProfileGraphDataAsync(parentProject);
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

        [HttpGet]
        public async Task<IActionResult> GetCumulativeGraphData(string parentProject)
        {
            var result = await _projectProfileService.GetCumulativeGraphDataAsync(parentProject);
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

        [HttpGet]
        public async Task<IActionResult> GetMonths()
        {
            var result = await _projectMonthService.GetMonthsAsync();
            if (!result.Success)
                return Json(new { success = false, message = "Failed to retrieve months." });

            return Json(new { success = true, data = result.Data });
        }

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

        [HttpDelete]
        public async Task<IActionResult> DeleteProjectMonth(string project, int monthNo)
        {
            var result = await _projectMonthService.DeleteProjectMonthAsync(project, monthNo);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete cost profile month." });
        }

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