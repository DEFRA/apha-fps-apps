using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class SubContractRmsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectSubContractService _subContractService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;

        public SubContractRmsController(
            IMapper mapper,
            IProjectSubContractService subContractService,
            IProjectService projectService,
            IMonthService monthService)
        {
            _mapper = mapper;
            _subContractService = subContractService;
            _projectService = projectService;
            _monthService = monthService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? month)
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 20,
                SortBy = "Project",
                Descending = false
            };

            if (month.HasValue)
            {
                defaultRequest.Filter = $"{{\"Month\":\"{month.Value}\"}}";
            }

            var monthsList = await GetMonthsListAsync();
            var projectsList = await GetProjectsListAsync();

            ViewBag.Projects = projectsList;
            ViewBag.Months = monthsList;

            return View(new SubContractRmsViewModel
            {
                Month = month,
                FilterMonths = monthsList,
                Projects = projectsList,
                SubContractsGrid = await BuildRmsSubContractGridAsync(defaultRequest, month)
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadRmsSubContractsGrid(PaginationFilter<string> request, int? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (month.HasValue)
            {
                var filterDict = string.IsNullOrEmpty(request.Filter)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? new Dictionary<string, string>();

                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var gridConfig = await BuildRmsSubContractGridAsync(request, month);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubContractRms(int id, int? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();
            await PopulateMonthsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditSubContractRms", new SubContractRmsItem
                {
                    Month = month
                });
            }

            var result = await _subContractService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var item = _mapper.Map<SubContractRmsItem>(result.Data);
            return PartialView("_AddEditSubContractRms", item);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubContractRms([FromBody] SubContractRmsItem model)
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

            var dto = _mapper.Map<ProjectSubContractDto>(model);
            ApiResponseDto<ProjectSubContractDto> result;
            string successMsg;

            if (model.SubContCounter == 0)
            {
                result = await _subContractService.CreateAsync(dto);
                successMsg = "Sub Contract saved successfully.";
            }
            else
            {
                result = await _subContractService.UpdateAsync(model.SubContCounter, dto);
                successMsg = "SubContract updated successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to save subcontract.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSubContractRms(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subContractService.DeleteAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete subcontract." });
        }

        // ── PRIVATE GRID BUILDERS ─────────────────────────────────────────────

        private async Task<DataGridConfig<SubContractRmsItem>> BuildRmsSubContractGridAsync(
            PaginationFilter<string> request, int? month = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            if (month.HasValue && !filterDict.ContainsKey("Month"))
            {
                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _subContractService.GetPagedProjectSubContractsManualAsync(query, null);

            var items = response.Data != null
                ? _mapper.Map<List<SubContractRmsItem>>(response.Data)
                : new List<SubContractRmsItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var queryParams = new List<string>();
            if (month.HasValue)
                queryParams.Add($"month={month.Value}");

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            return new DataGridConfig<SubContractRmsItem>
            {
                GridId = "rmsSubContractsGrid",
                Title = string.Empty,
                KeyProperty = "SubContCounter",
                AddFunction = "addSubContractRms",
                EditFunction = "editSubContractRms",
                DeleteFunction = "deleteSubContractRms",
                BindGridUrl = $"/PACT/SubContractRms/LoadRmsSubContractsGrid{queryString}",
                ExtraFilterMethod = "getRmsSubContractFilters",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<SubContractRmsItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<List<SelectListItem>> GetProjectsListAsync()
        {
            var result = await _projectService.GetAllPactProjectsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                var projectList = result.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject
                    })
                    .ToList();

                return projectList;
            }
            else
            {
                return new List<SelectListItem>();
            }
        }

        private async Task<List<SelectListItem>> GetMonthsListAsync()
        {
            var result = await _monthService.GetAllMonthsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                var monthList = result.Data
                    .OrderBy(m => m.Monthnumber)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Monthnumber.ToString(),
                        Text = $"{m.Monthnumber} - {m.Monthname}"
                    })
                    .ToList();

                return monthList;
            }
            else
            {
                return new List<SelectListItem>();
            }
        }

        private async Task PopulateProjectsViewBagAsync()
        {
            ViewBag.Projects = await GetProjectsListAsync();
        }

        private async Task PopulateMonthsViewBagAsync()
        {
            ViewBag.Months = await GetMonthsListAsync();
        }
    }
}
