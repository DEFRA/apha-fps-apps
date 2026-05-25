using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class WorkGroupTimeByJobCodeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;

        public WorkGroupTimeByJobCodeController(IMapper mapper, IWorkGroupService workGroupService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
        }

        public async Task<IActionResult> Index(string workGroup, string personName)
        {
            var grid    = BuildGrid();
            var summary = new WorkGroupTimeByJobCodeSummary();
            double hrsPaid = 0;

            if (!string.IsNullOrWhiteSpace(workGroup))
            {
                var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
                var query    = _mapper.Map<QueryParameters<string>>(defaultRequest);
                var response = await _workGroupService.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);
                if (response.Success && response.Data != null)
                {
                    grid.Data    = _mapper.Map<List<WorkGroupTimeByJobCodeRow>>(response.Data.Rows);
                    summary      = _mapper.Map<WorkGroupTimeByJobCodeSummary>(response.Data.Summary);
                    hrsPaid      = response.Data.HrsPaid;
                    grid.Pagination = new PaginationModel
                    {
                        TotalRecords = response.Data.Pagination.TotalRecords,
                        PageNumber   = response.Data.Pagination.PageNumber,
                        PageSize     = response.Data.Pagination.PageSize
                    };
                }
            }

            return View(new WorkGroupTimeByJobCodeViewModel
            {
                SelectedWorkGroup  = workGroup,
                SelectedPersonName = personName,
                WorkGroupName      = workGroup,
                HrsPaid            = hrsPaid,
                Grid               = grid,
                Summary            = summary
            });
        }

        /// <summary>AJAX POST — reloads the grid partial for a selected work group.</summary>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request, string workGroup, string personName)
        {
            var grid = BuildGrid();

            if (!string.IsNullOrWhiteSpace(workGroup))
            {
                var query    = _mapper.Map<QueryParameters<string>>(request);
                var response = await _workGroupService.GetWgSummarisedStaffTimeUsageAsync(query, workGroup);
                if (response.Success && response.Data != null)
                {
                    grid.Data = _mapper.Map<List<WorkGroupTimeByJobCodeRow>>(response.Data.Rows);
                    grid.Pagination = new PaginationModel
                    {
                        TotalRecords = response.Data.Pagination.TotalRecords,
                        PageNumber   = response.Data.Pagination.PageNumber,
                        PageSize     = response.Data.Pagination.PageSize,
                        SortColumn   = request.SortBy,
                        SortDirection = request.Descending
                    };
                }
            }

            return PartialView("_DataGrid", grid);
        }

        private static DataGridConfig<WorkGroupTimeByJobCodeRow> BuildGrid() => new()
        {
            GridId             = "timeUsageGrid",
            Title              = string.Empty,
            BindGridUrl        = "/PACT/WorkGroupTimeByJobCode/LoadGrid",
            ExtraFilterMethod  = "getWorkGroupTimeByJobCodeExtraFilters",
            ShowCheckboxColumn = false,
            AllowAdd           = false,
            AllowEdit          = false,
            AllowDelete        = false,
            AllowRowSelection  = false,
            ShowPagination     = true,
            Columns            = GridDataProvider.GetColumnsDefination<WorkGroupTimeByJobCodeRow>()
        };
    }
}
