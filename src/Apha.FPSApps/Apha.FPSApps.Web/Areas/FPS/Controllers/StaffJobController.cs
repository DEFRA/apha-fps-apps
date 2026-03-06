using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
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
        public async Task<IActionResult> LoadStaffJobGrid(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter);
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
                KeyProperty = "Id",
                AddUrl = "/FPS/StaffJob/StaffJob",
                UpdateUrl = "/FPS/StaffJob/EditStaffJob",
                DeleteUrl = "/FPS/StaffJob/Delete",
                BindGridUrl = "/FPS/StaffJob/LoadStaffJobGrid",                
                Data = staffJobItems,
                Columns = GridDataProvider.GetColumnsDefination<StaffJobItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return View("_DataGrid", staffJobGridConfig);
        }

        public IActionResult AddStaffJob()
        {
            return View("_AddStaffJob");
        }

        public IActionResult EditStaffJob()
        {
            return View("_EditStaffJob");
        }
    }
}
