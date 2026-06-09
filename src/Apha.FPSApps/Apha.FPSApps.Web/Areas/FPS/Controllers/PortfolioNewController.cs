using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class PortfolioNewController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;

        public PortfolioNewController(
            IMapper mapper,
            IProjectService projectService,
            IProgramService programService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _programService = programService;
        }

        // GET: /FPS/PortfolioNew/Index?parentProject=XYZ
        public async Task<IActionResult> Index(string parentProject)
        {
            var response = await _projectService.GetProjectByIdAsync(parentProject);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<PortfolioNewViewModel>(response.Data);
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // POST: /FPS/PortfolioNew/Index  (called via AJAX from the view)
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] PortfolioNewViewModel model)
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

            var dto = _mapper.Map<ProjectDto>(model);
            var response = await _projectService.UpdateFpsPortfolioAsync(dto);
            if (response.Success)
                return Json(new { success = true, data = response.Data, message = "Portfolio details saved successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save portfolio details.",
                errors = (response.Errors ?? new()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task PopulateDropdownsAsync(PortfolioNewViewModel model)
        {
            var managerTask = _projectService.GetManagersAsync();
            var statusTask = _projectService.GetAllStatusesAsync();
            var diseaseTask = _projectService.GetAllDiseasesAsync();
            var customerTask = _projectService.GetAllCustomersAsync();
            var contractTask = _projectService.GetContractsByUserAsync();
            var programTask = _programService.GetAllProgramsAsync();

            await Task.WhenAll(managerTask, statusTask, diseaseTask, customerTask, contractTask, programTask);

            var managers = (await managerTask).Data ?? new();
            model.ManagerList = managers
                .Where(m => !string.IsNullOrEmpty(m.Name))
                .Select(m => new SelectListItem($"{m.Name} | {m.WorkGroup ?? string.Empty}", m.Name, m.Name == model.Manager))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var statuses = (await statusTask).Data ?? new();
            model.ProjectStatusList = statuses
                .Where(s => !string.IsNullOrEmpty(s.Status))
                .Select(s => new SelectListItem(s.Status, s.Status, s.Status == model.ProjectStatus))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var diseases = (await diseaseTask).Data ?? new();
            model.DiseaseList = diseases
                .Where(d => !string.IsNullOrEmpty(d.Disease))
                .Select(d => new SelectListItem(d.Disease, d.Disease, d.Disease == model.Disease))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var customers = (await customerTask).Data ?? new();
            model.CustomerList = customers
                .Where(c => !string.IsNullOrEmpty(c.Customer))
                .Select(c => new SelectListItem(c.Customer, c.Customer, c.Customer == model.Customer))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var contracts = (await contractTask).Data ?? new();
            model.ContractList = contracts
                .Where(c => !string.IsNullOrEmpty(c.ContractNo))
                .Select(c => new SelectListItem(c.ContractNo, c.ContractNo, c.ContractNo == model.Contract))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var programs = (await programTask).Data ?? Enumerable.Empty<ProgramDto>();
            model.ProgramList = programs
                .Where(p => !string.IsNullOrEmpty(p.ProgramNo))
                .Select(p => new SelectListItem($"{p.ProgramNo} | {p.ProgramName ?? string.Empty}", p.ProgramNo, p.ProgramNo == model.Program))
                .Prepend(new SelectListItem("", ""))
                .ToList();
        }
    }
}
