using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class BosworthInterfaceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IBosworthInterfaceService _bosworthInterfaceService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IProjectService _projectService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IExcelExportService _excelExportService;

        public BosworthInterfaceController(
            IMapper mapper,
            IBosworthInterfaceService bosworthInterfaceService,
            IWorkGroupService workGroupService,
            IProjectService projectService,
            IProfitCentreService profitCentreService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _bosworthInterfaceService = bosworthInterfaceService;
            _workGroupService = workGroupService;
            _projectService = projectService;
            _profitCentreService = profitCentreService;
            _excelExportService = excelExportService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new BosworthInterfaceViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTimePurchaseProjectReport(string project)
        {   
            var response = await _bosworthInterfaceService.GetTimePurchaseProjectAsync(project);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TimePurchaseProject");
            var fileName = $"TimePurchaseProject_{project}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTimeSaleProfitCentreReport(string profitCentre)
        {
            var response = await _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(profitCentre);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TimeSaleProfitCentre");
            var fileName = $"TimeSaleProfitCentre_{profitCentre}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTimeSaleWorkgroupReport(string workGroup)
        {
            var response = await _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(workGroup);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TimeSaleWorkgroup");
            var fileName = $"TimeSaleWorkgroup_{workGroup}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTestSaleSellingWorkgroupReport(string workGroup)
        {
            var response = await _bosworthInterfaceService.GetTestSaleSellingWorkgroupAsync(workGroup);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TestSaleSellingWorkgroup");
            var fileName = $"TestSaleSellingWorkgroup_{workGroup}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTestSaleBuyingProjectReport(string parentProject)
        {
            var response = await _bosworthInterfaceService.GetTestSaleBuyingProjectAsync(parentProject);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TestSaleBuyingProject");
            var fileName = $"TestSaleBuyingProject_{parentProject}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task PopulateDropdownsAsync(BosworthInterfaceViewModel viewModel)
        {
            var projectsResponse = await _projectService.GetAllPactProjectsAsync();
            var profitCentresResponse = await _profitCentreService.GetAllProfitCentresAsync();
            var workGroupsResponse = await _workGroupService.GetAllWorkGroupsAsync();

            viewModel.ProjectOptions = projectsResponse.Success && projectsResponse.Data != null
                ? _mapper.Map<List<Project>>(projectsResponse.Data)
                    .OrderBy(p => p.ParentProject)
                    .ToList()
                : [];

            viewModel.ProfitCentreOptions = profitCentresResponse.Success && profitCentresResponse.Data != null
                ? _mapper.Map<List<ProfitCentre>>(profitCentresResponse.Data)
                    .OrderBy(pc => pc.Division)
                    .ToList()
                : [];

            viewModel.WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                ? _mapper.Map<List<WorkGroup>>(workGroupsResponse.Data)
                    .OrderBy(w => w.WorkGroupName)
                    .ToList()
                : [];
        }
    }
}