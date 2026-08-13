using Apha.Common.Utilities.GenericExcelExport;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Extensions;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class ProjectListController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectListService _projectListService;
        private readonly IGenericExcelExporter _excelExporter;

        public ProjectListController(
            IMapper mapper,
            IProjectListService projectListService,
            IGenericExcelExporter excelExporter)
        {
            _mapper = mapper;
            _projectListService = projectListService;
            _excelExporter = excelExporter;
        }

        public async Task<IActionResult> Index()
        {
            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            DataGridConfig<ProjectListItem> gridConfig = await BuildProjectListGridAsync(defaultRequest, 1);
            return View(new ProjectListViewModel { ProjectGrid = gridConfig, FilterOption = 1 });
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> LoadProjectListGrid(PaginationFilter<string> request, int filterOption = 2)
        {
            // DataGrid Excel Export hook — reuses the same filters and returns the full result set as .xlsx.
            if (this.IsExcelExportRequest())
            {
                List<ProjectListItem> exportItems = await GetProjectListItemsAsync(request, filterOption, exportAll: true);
                return this.ExcelFile(_excelExporter, exportItems, "ProjectList", "Projects");
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<ProjectListItem> gridConfig = await BuildProjectListGridAsync(request, filterOption);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<List<ProjectListItem>> GetProjectListItemsAsync(
            PaginationFilter<string> request, int filterOption, bool exportAll = false)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            if (exportAll)
            {
                queryParameters.Page = 1;
                queryParameters.PageSize = int.MaxValue;
            }

            var pagedData = await _projectListService.GetAllProjectsAsync(queryParameters, filterOption);
            if (pagedData.Success && pagedData.Data != null)
            {
                return _mapper.Map<List<ProjectListItem>>(pagedData.Data);
            }

            return new List<ProjectListItem>();
        }

        private async Task<DataGridConfig<ProjectListItem>> BuildProjectListGridAsync(PaginationFilter<string> request, int filterOption)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _projectListService.GetAllProjectsAsync(queryParameters, filterOption);

            List<ProjectListItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
            {
                items = _mapper.Map<List<ProjectListItem>>(pagedData.Data);
            }
            else if (pagedData.Errors != null)
            {
                foreach (var error in pagedData.Errors)
                    Console.WriteLine($"Project list error: {error.Message}");
            }

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectListItem>
            {
                GridId = "projectListGrid",
                Title = "Select a Project",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Parentproject",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                EditFunction = "editProject",
                AllowView = true,
                ViewFunction = "viewProject",
                AllowExcelExport = true,
                ExtraFilterMethod = "getProjectExtraFilters",
                BindGridUrl = "/PIMS/ProjectList/LoadProjectListGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectListItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}