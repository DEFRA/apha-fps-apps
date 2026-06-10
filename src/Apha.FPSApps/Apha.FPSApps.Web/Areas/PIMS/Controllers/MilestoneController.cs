using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Web;


namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class MilestoneController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMilestoneService _milestoneService;
        private readonly IProjectListService _projectListService;

        public MilestoneController(
            IMapper mapper,
            IMilestoneService milestoneService,
            IProjectListService projectListService)
        {
            _mapper = mapper;
            _milestoneService = milestoneService;
            _projectListService = projectListService;
        }

        public async Task<IActionResult> Index(string? parentproject = null)
        {
            MilestoneViewModel viewModel = new();
            ApiResponseDto<List<ProjectListMilestoneDto>> allProjects =
                await _projectListService.GetAllProjectsForMilestoneAsync();

            viewModel.ProjectOptions = allProjects.Data?
                .Select(p => new SelectListItem(p.Parentproject, p.Parentproject))
                .ToList() ?? [];

            string project = parentproject ?? viewModel.ProjectOptions.FirstOrDefault()?.Value ?? string.Empty;
            viewModel.Parentproject = project;
            viewModel.FormRequired = allProjects.Data?
                .FirstOrDefault(p => p.Parentproject == project)?.Formrequired ?? false;

            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            viewModel.MilestonesGrid = await BuildMilestonesGridAsync(project, defaultRequest);
            viewModel.MilestoneFormDatesGrid = await BuildMilestoneFormDatesGridAsync(project, defaultRequest);
            return View(viewModel);
        }

        // ── Milestones DataGrid ──────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadMilestoneGrid(
            PaginationFilter<string> request, string? parentproject = null)
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

            DataGridConfig<MilestoneItem> gridConfig =
                await BuildMilestonesGridAsync(parentproject ?? string.Empty, request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<MilestoneItem>> BuildMilestonesGridAsync(
            string parentproject, PaginationFilter<string> request)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _milestoneService.GetAllMilestonesAsync(queryParameters, parentproject);

            List<MilestoneItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
            {
                items = _mapper.Map<List<MilestoneItem>>(pagedData.Data);
            }
            else if (pagedData.Errors != null)
            {
                foreach (var error in pagedData.Errors)
                    Console.WriteLine($"Milestone error: {error.Message}");
            }

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<MilestoneItem>
            {
                GridId = "milestonesGrid",                
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Number",
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addMilestone",
                EditFunction = "editMilestone",
                DeleteFunction = "deleteMilestone",
                ExtraFilterMethod = "getMilestoneExtraFilters",
                BindGridUrl = "/PIMS/Milestone/LoadMilestoneGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<MilestoneItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
        private async Task<DataGridConfig<MilestoneFormDatesItem>> BuildMilestoneFormDatesGridAsync(
            string parentproject, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _milestoneService.GetAllMilestoneFormDatesAsync(parentproject, queryParameters);

            List<MilestoneFormDatesItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
            {
                items = _mapper.Map<List<MilestoneFormDatesItem>>(pagedData.Data);
            }
            else if (pagedData.Errors != null)
            {
                foreach (var error in pagedData.Errors)
                    Console.WriteLine($"MilestoneFormDates error: {error.Message}");
            }

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<MilestoneFormDatesItem>
            {
                GridId = "milestoneFormDatesGrid",               
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Year",
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addMilestoneFormDates",
                EditFunction = "editMilestoneFormDates",
                DeleteFunction = "deleteMilestoneFormDates",
                ExtraFilterMethod = "getMilestoneFormDatesExtraFilters",
                BindGridUrl = "/PIMS/Milestone/LoadMilestoneFormDatesGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<MilestoneFormDatesItem>(),
                Pagination = paginationModel
            };
        }

        [HttpPost]
        public async Task<IActionResult> LoadMilestoneFormDatesGrid(
           PaginationFilter<string> request, string? parentproject = null)
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

            DataGridConfig<MilestoneFormDatesItem> gridConfig =
                await BuildMilestoneFormDatesGridAsync(parentproject ?? string.Empty, request);
            return PartialView("_DataGrid", gridConfig);
        }
        [HttpGet]
        public async Task<IActionResult> GetAddEditMilestonePartial(string parentproject, string? number = null)
        {
            var decodedId = HttpUtility.UrlDecode(number);
            MilestoneItem model = new() { Project = parentproject };
            if (!string.IsNullOrWhiteSpace(decodedId))
            {
                ApiResponseDto<MilestoneDto> result =
                    await _milestoneService.GetMilestoneAsync(parentproject, decodedId);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<MilestoneItem>(result.Data);
            }

            List<SelectListItem> typeOptions = await GetMilestoneTypeOptionsAsync();
            ViewBag.MilestoneTypeOptions = typeOptions;
            ViewBag.IsAddingNew = string.IsNullOrWhiteSpace(number);
            return PartialView("_AddEditMilestone", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMilestone(MilestoneItem item)
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

            MilestoneDto dto = _mapper.Map<MilestoneDto>(item);

            ApiResponseDto<MilestoneDto> result = item.IsAddingNew
                ? await _milestoneService.SaveMilestoneAsync(item.Project, dto)
                : await _milestoneService.UpdateMilestoneAsync(item.Project, item.Number, dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = item.IsAddingNew ? "Milestone saved successfully." : "Milestone updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditMilestoneFormDatesPartial(
            string parentproject, short? year = null)
        {
            MilestoneFormDatesItem model = new() { ParentProject = parentproject };
            if (year.HasValue)
            {
                ApiResponseDto<MilestoneFormDatesDto> result =
                    await _milestoneService.GetMilestoneFormDatesAsync(parentproject, year.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<MilestoneFormDatesItem>(result.Data);
            }

            ViewBag.IsAddingNew = !year.HasValue;
            return PartialView("_AddEditMilestoneFormDates", model);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteMilestone(string parentproject, string number)
        {
            var decodedId = HttpUtility.UrlDecode(number);
            ApiResponseDto<object> result =
                await _milestoneService.DeleteMilestoneAsync(parentproject, decodedId);
            return result.Success
                ? Json(new { success = true, message = "Milestone deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMilestoneFormDates(MilestoneFormDatesItem item)
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

            MilestoneFormDatesDto dto = _mapper.Map<MilestoneFormDatesDto>(item);
            ApiResponseDto<MilestoneFormDatesDto> result =
                await _milestoneService.SaveMilestoneFormDatesAsync(item.ParentProject, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Financial year record saved successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMilestoneFormDates(string parentproject, short year)
        {
            ApiResponseDto<object> result =
                await _milestoneService.DeleteMilestoneFormDatesAsync(parentproject, year);
            return result.Success
                ? Json(new { success = true, message = "Financial year record deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFormRequired(string parentproject, bool formRequired)
        {
            ApiResponseDto<object> result =
                await _milestoneService.UpdateFormRequiredAsync(parentproject, formRequired);
            return result.Success
                ? Json(new { success = true })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> GetFormRequired(string parentproject)
        {
            ApiResponseDto<List<ProjectListMilestoneDto>> allProjects =
                await _projectListService.GetAllProjectsForMilestoneAsync();
            bool formRequired = allProjects.Data?
                .FirstOrDefault(p => p.Parentproject == parentproject)?.Formrequired ?? false;
            return Json(new { formRequired });
        }
        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(MilestoneViewModel viewModel)
        {
            ApiResponseDto<List<ProjectListMilestoneDto>> allProjects =
                await _projectListService.GetAllProjectsForMilestoneAsync();

            viewModel.ProjectOptions = allProjects.Data?
                .Select(p => new SelectListItem(p.Parentproject, p.Parentproject))
                .ToList() ?? [];
        }
        private async Task<List<SelectListItem>> GetMilestoneTypeOptionsAsync()
        {
            ApiResponseDto<List<MilestoneTypeDto>> types =
                await _milestoneService.GetMilestoneTypesAsync();
            return types.Data?
                .Select(t => new SelectListItem(t.Type, t.IdType.ToString()))
                .ToList() ?? [];
        }
    }
}
