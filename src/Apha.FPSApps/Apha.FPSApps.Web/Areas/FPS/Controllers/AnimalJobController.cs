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
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class AnimalJobController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAnimalPlanService _animalPlanService;

        public AnimalJobController(IMapper mapper, IAnimalPlanService animalPlanService)
        {
            _mapper = mapper;
            _animalPlanService = animalPlanService;
        }

        [HttpPost]
        public async Task<IActionResult> LoadAnimalPlanGrid(PaginationFilter<string> request, string? jobCode = null)
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
            ApiResponseDto<List<AnimalCostViewDto>> pagedData = await _animalPlanService.GetAllAnimalCostAsync(queryParameters, jobCode ?? string.Empty);

            List<AnimalPlanItem> animalItems = pagedData.Data != null
                ? _mapper.Map<List<AnimalPlanItem>>(pagedData.Data.ToList())
                : new List<AnimalPlanItem>();

            PaginationModel paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var animalCostGrid = new DataGridConfig<AnimalPlanItem>
            {
                GridId = "animalBookedGrid",
                Title = "Animal Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "IndCounter",
                AddFunction = "addAnimalPlan",
                EditFunction = "editAnimalPlan",
                DeleteFunction = "deleteAnimalPlan",
                ExtraFilterMethod = "getAnimalPlanExtraFilters",
                BindGridUrl = "/FPS/AnimalJob/LoadAnimalPlanGrid",
                Data = animalItems,
                Columns = GridDataProvider.GetColumnsDefination<AnimalPlanItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", animalCostGrid);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            AnimalPlanItem model = new AnimalPlanItem();
            await PopulateAnimalDropdownAsync(model);
            return PartialView("_AddEditAnimalPlan", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnimalPlanItem animalPlanItem)
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

            AnimalRequestDto dto = _mapper.Map<AnimalRequestDto>(animalPlanItem);
            ApiResponseDto<AnimalRequestDto> result = await _animalPlanService.CreateAnimalCostAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Animal cost created successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create animal cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int indCounter, string? jobCode = null)
        {
            ApiResponseDto<AnimalCostViewDto?> result = await _animalPlanService.GetAnimalCostViewByIdAsync(
                indCounter, jobCode ?? string.Empty);

            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to retrieve animal cost details.",
                    errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });
            }

            AnimalPlanItem model = _mapper.Map<AnimalPlanItem>(result.Data);
            await PopulateAnimalDropdownAsync(model);
            return PartialView("_AddEditAnimalPlan", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int indCounter, [FromBody] AnimalPlanItem animalPlanItem)
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

            AnimalRequestDto dto = _mapper.Map<AnimalRequestDto>(animalPlanItem);
            dto.IndCounter = indCounter;
            ApiResponseDto<AnimalRequestDto> result = await _animalPlanService.UpdateAnimalCostAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Animal cost updated successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update animal cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int indCounter)
        {
            ApiResponseDto<bool> result = await _animalPlanService.DeleteAnimalCostAsync(indCounter);
            if (result.Success)
            {
                return Json(new { success = true, message = "Animal cost deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete animal cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAnimalRate(string animalType, string jobCode)
        {
            if (string.IsNullOrWhiteSpace(animalType))
            {
                return Json(new { success = false, message = "Animal type is required", dailyRate = 0 });
            }

            ApiResponseDto<decimal?> result = await _animalPlanService.GetAnimalRateAsync(animalType, jobCode);

            if (result.Success)
            {
                return Json(new { success = true, dailyRate = result.Data ?? 0 });
            }

            return Json(new
            {
                success = false,
                message = "Failed to retrieve animal rate.",
                dailyRate = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalAnimalCost(string jobCode)
        {
            if (string.IsNullOrWhiteSpace(jobCode))
            {
                return Json(new { success = false, message = "Job Code is required", totalAnimalCost = 0 });
            }

            ApiResponseDto<decimal> result = await _animalPlanService.GetTotalAnimalCostAsync(jobCode);

            if (result.Success)
            {
                return Json(new { success = true, totalAnimalCost = result.Data });
            }

            return Json(new
            {
                success = false,
                message = "Failed to retrieve total animal cost.",
                totalAnimalCost = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task PopulateAnimalDropdownAsync(AnimalPlanItem model)
        {
            ApiResponseDto<List<AnimalDto>> animalResponse = await _animalPlanService.GetAnimalLookupAsync();
            model.AnimalTypeList = animalResponse.Data == null ? new List<SelectListItem>() :
                animalResponse.Data
                    .Select(a => new SelectListItem
                    {
                        Value = a.AnimalType,
                        Text = a.AnimalType,
                        Selected = string.Equals(model.AnimalType, a.AnimalType, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
        }
    }
}
