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
    public class ResourceSetUpController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IResourceCentreGradeService _rcGradeService;
        private readonly IWorkGroupGradeService _wgGradeService;
        private readonly IWorkGroupEmployeeService _WorkGroupEmployeeService;

        public ResourceSetUpController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IResourceCentreGradeService rcGradeService,
            IWorkGroupGradeService wgGradeService,
            IWorkGroupEmployeeService WorkGroupEmployeeService)
        {
            _mapper = mapper;
            _profitCentreService = profitCentreService;
            _rcGradeService = rcGradeService;
            _wgGradeService = wgGradeService;
            _WorkGroupEmployeeService = WorkGroupEmployeeService;
        }

        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var profitCentreList = await GetProfitCentreListAsync();
            var selectedProfitCentre = profitCentre ?? string.Empty;

            var viewModel = new ResourceSetUpViewModel
            {
                ProfitCentre     = selectedProfitCentre,
                ProfitCentreList = profitCentreList
            };

            // Load RC grades server-side when a profit centre is selected (same pattern as ProgramStaffPlan)
            var rcGradeItems = new List<RcGradeItem>();
            if (!string.IsNullOrWhiteSpace(selectedProfitCentre))
            {
                var rcResponse = await _rcGradeService.GetResourceCentreGradesAsync(selectedProfitCentre);
                if (rcResponse.Success && rcResponse.Data != null)
                {
                    rcGradeItems = rcResponse.Data
                        .Select(d => new RcGradeItem
                        {
                            PcGrade        = d.PcGrade,
                            RcGradeDisplay = d.PcGrade,
                            ChargeRate     = d.ChargeRate
                        })
                        .ToList();
                }
            }

            viewModel.RcGradeGrid = new DataGridConfig<RcGradeItem>
            {
                GridId             = "rcGradeGrid",
                Title              = "RC Grades Available",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "PcGrade",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getRcGradeExtraFilters",
                BindGridUrl        = "/FPS/ResourceSetUp/LoadRcGradeGrid",
                Data               = rcGradeItems.Take(10).ToList(),
                Columns            = GridDataProvider.GetColumnsDefination<RcGradeItem>(),
                Pagination         = new PaginationModel { TotalRecords = rcGradeItems.Count, PageNumber = 1, PageSize = 10 }
            };

            viewModel.WgGradeGrid = new DataGridConfig<WgGradeItem>
            {
                GridId             = "wgGradeGrid",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "WgGrade",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getWgGradeExtraFilters",
                BindGridUrl        = "/FPS/ResourceSetUp/LoadWgGradeGrid",
                Data               = new List<WgGradeItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<WgGradeItem>(),
                Pagination         = new PaginationModel()
            };

            viewModel.WgStaffGrid = new DataGridConfig<WgStaffItem>
            {
                GridId             = "wgStaffGrid",
                Title              = "Staff of WG Grade",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "PactId",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editWgStaff",
                AllowDelete        = false,
                ExtraFilterMethod  = "getWgStaffExtraFilters",
                BindGridUrl        = "/FPS/ResourceSetUp/LoadWgStaffGrid",
                Data               = new List<WgStaffItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<WgStaffItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadRcGradeGrid(string profitCentre, int page = 1, int pageSize = 10, string? filter = null, string? sortBy = null, bool descending = false)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                return Json(new { success = false, message = "Profit Centre is required." });
            }

            var response = await _rcGradeService.GetResourceCentreGradesAsync(profitCentre);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load RC grades." });
            }

            var items = (response.Data ?? new List<ProfitCentreGradeDto>())
                .Select(d => new RcGradeItem
                {
                    PcGrade        = d.PcGrade,
                    RcGradeDisplay = d.PcGrade,
                    ChargeRate     = d.ChargeRate
                })
                .AsQueryable();

            var filterDict = ParseFilterJson(filter);
            if (filterDict.TryGetValue("RcGradeDisplay", out var rcGradeFilter) && !string.IsNullOrWhiteSpace(rcGradeFilter))
                items = items.Where(x => x.RcGradeDisplay.Contains(rcGradeFilter, StringComparison.OrdinalIgnoreCase));

            items = sortBy?.ToLower() switch
            {
                "rcgradedisplay" => descending ? items.OrderByDescending(x => x.RcGradeDisplay) : items.OrderBy(x => x.RcGradeDisplay),
                "chargerate"    => descending ? items.OrderByDescending(x => x.ChargeRate)     : items.OrderBy(x => x.ChargeRate),
                _               => items
            };

            var totalRecords = items.Count();
            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var gridConfig = new DataGridConfig<RcGradeItem>
            {
                GridId             = "rcGradeGrid",
                Title              = "RC Grades Available",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "PcGrade",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getRcGradeExtraFilters",
                BindGridUrl        = "/FPS/ResourceSetUp/LoadRcGradeGrid",
                Data               = pagedItems,
                Columns            = GridDataProvider.GetColumnsDefination<RcGradeItem>(),
                Pagination         = new PaginationModel { TotalRecords = totalRecords, PageNumber = page, PageSize = pageSize, SortColumn = sortBy, SortDirection = descending },
                CurrentFilters     = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadWgGradeGrid(string pcGrade, int page = 1, int pageSize = 10, string? filter = null, string? sortBy = null, bool descending = false)
        {
            if (string.IsNullOrWhiteSpace(pcGrade))
            {
                return Json(new { success = false, message = "RC Grade is required." });
            }

            var response = await _wgGradeService.GetWorkGroupGradeAsync(pcGrade);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load WG grades." });
            }

            var items = (response.Data ?? new List<WorkgroupGradeDto>())
                .Select(d => new WgGradeItem
                {
                    ProfitCentreGrade = d.ProfitCentreGrade,
                    WgGrade           = d.WgGrade
                })
                .AsQueryable();

            var filterDict = ParseFilterJson(filter);
            if (filterDict.TryGetValue("WgGrade", out var wgGradeFilter) && !string.IsNullOrWhiteSpace(wgGradeFilter))
                items = items.Where(x => x.WgGrade.Contains(wgGradeFilter, StringComparison.OrdinalIgnoreCase));

            items = sortBy?.ToLower() switch
            {
                "wggrade" => descending ? items.OrderByDescending(x => x.WgGrade) : items.OrderBy(x => x.WgGrade),
                _         => items
            };

            var totalRecords = items.Count();
            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var gridConfig = new DataGridConfig<WgGradeItem>
            {
                GridId             = "wgGradeGrid",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "WgGrade",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getWgGradeExtraFilters",
                BindGridUrl        = "/FPS/ResourceSetUp/LoadWgGradeGrid",
                Data               = pagedItems,
                Columns            = GridDataProvider.GetColumnsDefination<WgGradeItem>(),
                Pagination         = new PaginationModel { TotalRecords = totalRecords, PageNumber = page, PageSize = pageSize, SortColumn = sortBy, SortDirection = descending },
                CurrentFilters     = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadWgStaffGrid(PaginationFilter<string> request, string wgGrade)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors  = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            if (string.IsNullOrWhiteSpace(wgGrade))
            {
                return Json(new { success = false, message = "WG Grade is required." });
            }

            var filterDict = ParseFilterJson(request.Filter);
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _WorkGroupEmployeeService.GetWorkGroupEmployeeAsync(queryParameters, wgGrade);

            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load WG staff." });
            }

            var rawData = response.Data ?? new List<WgEmployeeViewDto>();
            var staffItems = rawData.Select(d => new WgStaffItem
            {
                PactId        = d.PactId,
                SpNumber      = d.SpNumber,
                Name          = d.Name,
                HrsPaid       = d.HrsPaid,
                Leave         = d.Leave,
                SickSpecial   = d.SickSpecial,
                HrsAvail      = d.HrsAvail,
                MakeAvailable = d.MakeAvailable == -1,
                PersonStatus  = d.PersonStatus,
                PersonClass   = d.PersonClass
            }).ToList();

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<WgStaffItem>
            {
                GridId             = "wgStaffGrid",
                Title              = "Staff of WG Grade",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "PactId",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editWgStaff",
                AllowDelete        = false,
                ExtraFilterMethod  = "getWgStaffExtraFilters",
                BindGridUrl        = "/FPS/ResourceSetUp/LoadWgStaffGrid",
                Data               = staffItems,
                Columns            = GridDataProvider.GetColumnsDefination<WgStaffItem>(),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> EditWgStaff(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                return Json(new { success = false, message = "PACTid is required." });
            }

            var response = await _WorkGroupEmployeeService.GetWorkGroupEmployeeByIdAsync(pactId);
            if (!response.Success || response.Data == null)
            {
                return Json(new { success = false, message = "WG Employee not found." });
            }

            var item = new WgStaffItem
            {
                PactId        = response.Data.PactId,
                SpNumber      = response.Data.SpNumber,
                Name          = response.Data.Name,
                HrsPaid       = response.Data.HrsPaid,
                Leave         = response.Data.Leave,
                SickSpecial   = response.Data.SickSpecial,
                HrsAvail      = response.Data.HrsAvail,
                MakeAvailable = response.Data.MakeAvailable == -1,
                PersonStatus  = response.Data.PersonStatus,
                PersonClass   = response.Data.PersonClass
            };

            return PartialView("_AddEditResourceSetUp", item);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWgStaff([FromBody] WgStaffItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors  = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = new WgEmployeeDto
            {
                PactId         = item.PactId,
                SpNumber       = item.SpNumber,
                WorkGroupGrade = string.Empty,
                HrsPaid        = item.HrsPaid,
                Leave          = item.Leave,
                SickSpecial    = item.SickSpecial,
                HrsAvail       = item.HrsAvail,
                MakeAvailable  = item.MakeAvailable ? -1 : 0,
                PersonStatus   = item.PersonStatus,
                PersonClass    = item.PersonClass
            };

            var result = await _WorkGroupEmployeeService.UpdateWorkGroupEmployeeAsync(dto);
            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Staff record updated successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update staff record.",
                errors  = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

       

        private static Dictionary<string, string> ParseFilterJson(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return new Dictionary<string, string>();
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                       ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private async Task<List<SelectListItem>> GetProfitCentreListAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProfitCentreId,
                        Text  = $"{p.ProfitCentreId} - {p.ProfitCentreName}"
                    })
                    .ToList();
            }
            return new List<SelectListItem>();
        }
    }
}
