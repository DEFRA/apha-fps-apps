using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
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
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class AnimalMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAnimalService _animalMasterService;

        public AnimalMaintenanceController(IMapper mapper, IAnimalService animalMasterService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _animalMasterService = animalMasterService ?? throw new ArgumentNullException(nameof(animalMasterService));
        }

        public async Task<IActionResult> Index()
        {
            var gridConfig = await GetAnimalMasterGridConfigAsync();
            return View(gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAnimalMasterGrid(PaginationFilter<string> request)
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
            var gridConfig = await GetAnimalMasterGridConfigAsync(queryParameters, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // GET: Create
        public IActionResult Create()
        {
            var model = new AnimalMaintenanceViewModel { AnimalType = string.Empty };
            return PartialView("_AddEditAnimalMaintenance", model);
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnimalMaintenanceViewModel model)
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

            var dto = _mapper.Map<AnimalDto>(model);
            var response = await _animalMasterService.AddAnimalAsync(dto);
            if (response.Success)
                return Json(new { success = true, data = response.Data, message = "Animal created successfully" });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create animal.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string animalType)
        {
            var response = await _animalMasterService.GetAnimalByIdAsync(animalType);
            if (!response.Success || response.Data == null)
                return NotFound();
            var model = _mapper.Map<AnimalMaintenanceViewModel>(response.Data);
            return PartialView("_AddEditAnimalMaintenance", model);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] AnimalMaintenanceViewModel model)
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

            var dto = _mapper.Map<AnimalDto>(model);
            var response = await _animalMasterService.UpdateAnimalAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Animal updated successfully.", data = response.Data });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update animal.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // DELETE
        [HttpDelete]
        public async Task<IActionResult> Delete(string animalType)
        {
            var response = await _animalMasterService.DeleteAnimalAsync(animalType);
            if (response.Success)
                return Json(new { success = true, message = "Animal deleted successfully.", data = response.Data });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete animal.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task<DataGridConfig<AnimalMaintenanceViewModel>> GetAnimalMasterGridConfigAsync(
            QueryParameters<string>? query = null,
            Dictionary<string, string>? filterDict = null)
        {
            var response = await _animalMasterService.GetAllAnimalsAsync(query ?? new QueryParameters<string>());
            var items = new List<AnimalMaintenanceViewModel>();
            if (response.Data != null)
                items = _mapper.Map<List<AnimalMaintenanceViewModel>>(response.Data.ToList());

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            return new DataGridConfig<AnimalMaintenanceViewModel>
            {
                GridId = "animalMasterGrid",
                Title = "Animal Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "AnimalType",
                AddFunction = "addAnimalMaster",
                EditFunction = "editAnimalMaster",
                DeleteFunction = "deleteAnimalMaster",
                BindGridUrl = "/FPS/AnimalMaintenance/LoadAnimalMasterGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AnimalMaintenanceViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
