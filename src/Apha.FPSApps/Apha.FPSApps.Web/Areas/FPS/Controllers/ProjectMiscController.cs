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
    /// <summary>
    /// MVC controller for Miscellaneous Project Data maintenance.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectMiscController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public ProjectMiscController(IMapper mapper, IProjectService projectService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }

        /// <summary>
        /// Displays the Misc Project Data page with the DataGrid.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var gridConfig = await GetProjectMiscGridConfigAsync();

            var viewModel = new ProjectMiscViewModel
            {
                ProjectMiscGrid = gridConfig
            };

            return View(viewModel);
        }

        /// <summary>
        /// Loads the Misc Project Data grid via AJAX for pagination and filtering.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectMiscGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetProjectMiscGridConfigAsync(queryParameters, filterDict);

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Returns the edit partial view populated with the project's misc data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                return Json(new { success = false, message = "Project is required." });

            var result = await _projectService.GetProjectByIdAsync(parentProject);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Project is not found." });

            var model = _mapper.Map<ProjectMiscItem>(result.Data);

            var subAccountResult = await _projectService.GetSubAccountsAsync();
            var subAccounts = subAccountResult.Data ?? new();
            model.SubAccountCodeList = subAccounts
                .Where(sa => !string.IsNullOrEmpty(sa.SubAccountCode))
                .Select(sa => new SelectListItem(
                    $"{sa.SubAccountCode} - {sa.SubAccount ?? string.Empty}",
                    sa.SubAccountCode,
                    sa.SubAccountCode == model.SubAccountCode))
                .Prepend(new SelectListItem("-- Select --", ""))
                .ToList();

            return PartialView("_EditProjectMisc", model);
        }

        /// <summary>
        /// Saves updates to a project's miscellaneous data fields.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProjectMiscItem item)
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

            var getResult = await _projectService.GetProjectByIdAsync(item.ParentProject);
            if (!getResult.Success || getResult.Data == null)
                return Json(new { success = false, message = $"Project '{item.ParentProject}' not found." });

            var projectDto = getResult.Data;
            projectDto.Program = item.Program;
            projectDto.CostCentre = item.CostCentre;
            projectDto.OracleProjectCode = item.OracleProjectCode;
            projectDto.SubAccountCode = item.SubAccountCode;

            var updateResult = await _projectService.UpdateProjectAsync(item.ParentProject, projectDto);
            if (updateResult.Success)
                return Json(new { success = true, message = "Project data updated successfully.", data = updateResult.Data });

            return Json(new
            {
                success = false,
                message = updateResult.Errors?.FirstOrDefault()?.Message ?? "Failed to update project data.",
                errors = (updateResult.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes a project.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projectService.DeleteProjectAndChildrenAsync(parentProject);
            if (result.Success && result.Data)
                return Json(new { success = true, message = "Project deleted successfully.", data = result.Data });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Unable to delete this project as it may be in use.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task<DataGridConfig<ProjectMiscItem>> GetProjectMiscGridConfigAsync(QueryParameters<string>? query = null, Dictionary<string, string>? filterDict = null)
        {
            var response = await _projectService.GetPagedProjectsAsync(query ?? new QueryParameters<string>());

            var items = new List<ProjectMiscItem>();
            if (response.Success && response.Data != null)
                items = _mapper.Map<List<ProjectMiscItem>>(response.Data);

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            return new DataGridConfig<ProjectMiscItem>
            {
                GridId = "projectMiscGrid",
                Title = "Misc Project Data",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "ParentProject",
                EditFunction = "editProjectMisc",
                DeleteFunction = "deleteProjectMisc",
                BindGridUrl = "/FPS/ProjectMisc/LoadProjectMiscGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectMiscItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
