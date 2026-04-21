using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProgramTestPlanController : Controller
    {
        private readonly IProgramService _programService;

        public ProgramTestPlanController(IProgramService programService)
        {
            _programService = programService;
        }

        public async Task<IActionResult> Index(string? programNo = null)
        {
            List<SelectListItem> programmeList = await GetProgrammeListAsync();
            string selectedProgramNo = programNo ?? string.Empty;
            ProgramDto? programInfo = await GetProgramInfoAsync(selectedProgramNo);

            var projectsGrid = new DataGridConfig<ProjectViewModel>
            {
                GridId = "projectGrid",
                Title = "Projects",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectProject",
                KeyProperty = "ParentProject",
                ExtraFilterMethod = "getProjectExtraFilters",
                BindGridUrl = "/FPS/ProgramProject/LoadProjectGrid",
                Data = new List<ProjectViewModel>(),
                Columns = GridDataProvider.GetColumnsDefination<ProjectViewModel>(),
                Pagination = new PaginationModel()
            };

            var testPlanGrid = new DataGridConfig<TestPlanItem>
            {
                GridId = "testPlanGrid",
                Title = "Test Purchase Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowDelete = true,
                KeyProperty = "TestCode",
                AddFunction = "addTestPlan",
                EditFunction = "editTestPlan",
                DeleteFunction = "deleteTestPlan",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = "/FPS/TestPlanJob/LoadTestPlanGrid",
                Data = new List<TestPlanItem>(),
                Columns = GridDataProvider.GetColumnsDefination<TestPlanItem>(null),
                Pagination = new PaginationModel()
            };

            var model = new ProgramTestPlanViewModel
            {
                SelectedProgramNo = selectedProgramNo,
                SelectedProgramme = programInfo?.ProgramName ?? string.Empty,
                Manager = programInfo?.Manager ?? string.Empty,
                Target = programInfo?.Target ?? 0,
                ProgrammeList = programmeList,
                ProjectsGrid = projectsGrid,
                TestPlanGrid = testPlanGrid
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProgramInfo(string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
            {
                return Json(new { success = false, message = "Programme number is required." });
            }

            ProgramDto? programInfo = await GetProgramInfoAsync(programNo);
            if (programInfo != null)
            {
                return Json(new
                {
                    success = true,
                    programmeName = programInfo.ProgramName,
                    manager = programInfo.Manager,
                    target = programInfo.Target ?? 0
                });
            }

            return Json(new { success = false, message = "Programme not found." });
        }

        private async Task<List<SelectListItem>> GetProgrammeListAsync()
        {
            var result = await _programService.GetAllProgramsAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProgramNo,
                        Text = $"{p.ProgramNo} - {p.ProgramName}"
                    })
                    .ToList();
            }

            return new List<SelectListItem>();
        }

        private async Task<ProgramDto?> GetProgramInfoAsync(string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
            {
                return null;
            }

            var result = await _programService.GetProgramByIdAsync(programNo);
            return result.Success ? result.Data : null;
        }
    }
}
