using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class ActualAnimalCostController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectSubContractService _projectSubContractService;
        private readonly IProjectService _projectService;

        public ActualAnimalCostController(
            IMapper mapper,
            IProjectSubContractService projectSubContractService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _projectSubContractService = projectSubContractService;
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> LoadActualAnimalCostGrid(PaginationFilter<string> request, string? projectCode = null)
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

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var pagedData = await _projectSubContractService.GetAnimalSubContractsAsync(queryParameters, projectCode);

            List<ActualAnimalCostItem> items = pagedData.Data != null
                ? _mapper.Map<List<ActualAnimalCostItem>>(pagedData.Data)
                : new List<ActualAnimalCostItem>();

            PaginationModel paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<ActualAnimalCostItem>
            {
                GridId = "actualAnimalCostGrid",
                Title = "Actual Animal Costs (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "SubContCounter",
                DeleteFunction = "deleteActualAnimalCost",
                ExtraFilterMethod = "getActualAnimalExtraFilters",
                BindGridUrl = "/FPS/ActualAnimalCost/LoadActualAnimalCostGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ActualAnimalCostItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectInfo(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            ApiResponseDto<ProjectDto> result = await _projectService.GetProjectByIdAsync(projectCode);
            if (result.Success && result.Data != null)
            {
                return Json(new
                {
                    success = true,
                    projectTitle = result.Data.ProjectTitle,
                    program = result.Data.Program,
                    contract = result.Data.Contract
                });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Project not found.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalActualCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required.", totalActualCost = 0 });

            ApiResponseDto<decimal> result = await _projectSubContractService.GetAnimalTotalAmountAsync(projectCode);
            if (result.Success)
                return Json(new { success = true, totalActualCost = result.Data });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve actual cost.",
                totalActualCost = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int subContCounter)
        {
            ApiResponseDto<bool> result = await _projectSubContractService.DeleteAsync(subContCounter);
            if (result.Success)
                return Json(new { success = true, message = "Animal sub-contract deleted successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete animal sub-contract.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }
    }
}
