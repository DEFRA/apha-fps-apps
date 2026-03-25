using Apha.FPSApps.Application.Dtos.FPS;
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
    public class StaffJobController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;

        public StaffJobController(IMapper mapper, IStaffJobService staffJobService)
        {
            _mapper = mapper;
            _staffJobService = staffJobService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffJobGrid(PaginationFilter<string> request, string? jobCode = null)
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

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter!);
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var staffJobPagedData = await _staffJobService.GetAllStaffJobsAsync(queryParameters);
            List<StaffJobItem> staffJobItems = new List<StaffJobItem>();
            if (staffJobPagedData.Data != null)
            {
                staffJobItems = _mapper.Map<List<StaffJobItem>>(staffJobPagedData.Data.ToList());
            }
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(staffJobPagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var staffJobGridConfig = new DataGridConfig<StaffJobItem>
            {
                GridId = "staffBookedGrid",
                Title = "Staff Booked",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "StaffID",
                AddFunction = "addStaffJob",
                EditFunction = "editStaffJob",
                DeleteFunction = "deleteStaffJob",
                ExtraFilterMethod = "getStaffJobExtraFilters",
                BindGridUrl = "/FPS/StaffJob/LoadStaffJobGrid",
                Data = staffJobItems,
                Columns = GridDataProvider.GetColumnsDefination<StaffJobItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", staffJobGridConfig);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddStaffJob");
        }

        [HttpPost]
        public async Task<IActionResult> Create(StaffJobViewModel staffJobItem)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid staff job data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }
            var staffJob = _mapper.Map<StaffJobDto>(staffJobItem);
            var result = await _staffJobService.CreateStaffJobAsync(staffJob);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Staff job created successfully" });
            }

            return Json(new { success = false, errors = result.Errors });            
        }  

        [HttpGet]
        public async Task<IActionResult> Edit(string staffId)
        {
            var result = await _staffJobService.GetStaffJobByIdAsync(staffId);

            if (result.Success)
            {
                var staffJobItem = _mapper.Map<StaffJobItem>(result.Data);
                return PartialView("_EditStaffJob", staffJobItem);
            }
            else
            {
                return NotFound($"Staff job with ID {staffId} not found.");
            }
        }          
       
        [HttpPost]
        public async Task<IActionResult> Edit(string staffId, [FromBody] StaffJobViewModel staffJobItem)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid staff job data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobItem);
            var result = await _staffJobService.UpdateStaffJobAsync(staffId, staffJobDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Staff job updated successfully" });
            }

            return Json(new { success = false, errors = result.Errors });
        }       
        
        [HttpDelete]
        public async Task<IActionResult> Delete(string staffId, string jobCode)
        {
            if (string.IsNullOrWhiteSpace(staffId))
            {
                return Json(new { success = false, message = "Staff ID is required" });
            }

            if (string.IsNullOrWhiteSpace(jobCode))
            {
                return Json(new { success = false, message = "Job code is required" });
            }

            var result = await _staffJobService.DeleteStaffJobAsync(staffId, jobCode);

            if (result.Success)
            {
                return Json(new { success = true, message = "Staff job deleted successfully" });
            }

            return Json(new { success = false, errors = "" });
        }
    }
}
