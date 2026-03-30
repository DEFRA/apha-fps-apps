using Apha.FPSApps.Application.Interfaces;
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
    public class ProgramProjectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public ProgramProjectController(IMapper mapper, IProjectService projectService)
        {
            _mapper = mapper;
            _projectService = projectService;
        }

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
                KeyProperty = "JobCode",
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
    }
}
