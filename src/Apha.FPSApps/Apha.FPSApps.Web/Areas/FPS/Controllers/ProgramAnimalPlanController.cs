using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.StateManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProgramAnimalPlanController : Controller
    {
        private readonly IProgramService _programService;
        private readonly IAppStateService _appStateService;

        public ProgramAnimalPlanController(IProgramService programService, IAppStateService appStateService)
        {
            _programService = programService;
            _appStateService = appStateService;
        }

        public async Task<IActionResult> Index(string? programNo = null)
        {
            List<SelectListItem> programmeList = await GetProgrammeListAsync();

            // Fall back to session if no programNo provided
            if (string.IsNullOrWhiteSpace(programNo))
                programNo = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProgramNo);

            string selectedProgramNo = !string.IsNullOrWhiteSpace(programNo) && programmeList.Any(p => p.Value == programNo)
                ? programNo
                : programmeList.FirstOrDefault()?.Value ?? string.Empty;

            await _appStateService.SetSessionAsync(SessionKeys.SelectedProgramNo, selectedProgramNo);

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

            var animalCostGrid = new DataGridConfig<AnimalPlanItem>
            {
                GridId = "animalBookedGrid",
                Title = "Animal Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowDelete = false,
                KeyProperty = "IndCounter",
                AddFunction = "addAnimalPlan",
                EditFunction = "editAnimalPlan",
                DeleteFunction = "deleteAnimalPlan",
                ExtraFilterMethod = "getAnimalPlanExtraFilters",
                BindGridUrl = $"/FPS/AnimalJob/LoadAnimalPlanGrid?title={Uri.EscapeDataString("Animal Plan")}", 
                Data = new List<AnimalPlanItem>(),
                Columns = GridDataProvider.GetColumnsDefination<AnimalPlanItem>(null),
                Pagination = new PaginationModel()
            };

            var model = new ProgramAnimalPlanViewModel
            {
                SelectedProgramNo = selectedProgramNo,
                SelectedProgramme = programInfo?.ProgramName ?? string.Empty,
                Manager = programInfo?.Manager ?? string.Empty,
                Target = programInfo?.Target ?? 0,
                ProgrammeList = programmeList,
                ProjectsGrid = projectsGrid,
                AnimalCostGrid = animalCostGrid
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
                return null;

            var result = await _programService.GetProgramByIdAsync(programNo);
            return result.Success ? result.Data : null;
        }
    }
}
