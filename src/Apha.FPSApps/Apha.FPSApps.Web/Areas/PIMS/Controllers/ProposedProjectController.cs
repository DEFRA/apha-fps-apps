using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class ProposedProjectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectListService _projectListService;

        public ProposedProjectController(IMapper mapper, IProjectListService projectListService)
        {
            _mapper = mapper;
            _projectListService = projectListService;
        }

        public async Task<IActionResult> Index()
        {
            ProposedProjectViewModel viewModel = await BuildViewModelAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProposedProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ProposedProjectViewModel viewModel = await BuildViewModelAsync();
                viewModel.Parentproject = model.Parentproject;
                viewModel.Projecttitle = model.Projecttitle;
                viewModel.Projectstatus = model.Projectstatus;
                viewModel.Costbookno = model.Costbookno;
                viewModel.Disease = model.Disease;
                viewModel.Program = model.Program;
                viewModel.Customer = model.Customer;
                viewModel.Manager = model.Manager;
                viewModel.Reason = model.Reason;
                return View("Index", viewModel);
            }

            ProposedProjectDto dto = _mapper.Map<ProposedProjectDto>(model);
            var result = await _projectListService.CreateProjectAsync(dto);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Project created successfully.";
                return RedirectToAction("Index", "ProjectList", new { area = "PIMS" });
            }
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Message ?? "An error occurred.");
                    TempData["Error"] = error.Message ?? "An error occurred."; // ← triggers showGovukAlert for errors
                }
            }

            ProposedProjectViewModel failedVm = await BuildViewModelAsync();
            failedVm.Parentproject = model.Parentproject;
            failedVm.Projecttitle = model.Projecttitle;
            failedVm.Projectstatus = model.Projectstatus;
            failedVm.Costbookno = model.Costbookno;
            failedVm.Disease = model.Disease;
            failedVm.Program = model.Program;
            failedVm.Customer = model.Customer;
            failedVm.Manager = model.Manager;
            failedVm.Reason = model.Reason;

            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Message ?? "An error occurred.");
            }

            return View("Index", failedVm);
        }

        private async Task<ProposedProjectViewModel> BuildViewModelAsync()
        {
            var programsTask = _projectListService.GetProjectProgramsAsync();
            var customersTask = _projectListService.GetProjectCustomersAsync();
            var statusesTask = _projectListService.GetProjectStatusesAsync();

            await Task.WhenAll(programsTask, customersTask, statusesTask);

            List<SelectListItem> programOptions = [new SelectListItem("-- Select program --", "")];
            if (programsTask.Result.Data != null)
                programOptions.AddRange(programsTask.Result.Data.Select(p => new SelectListItem(p, p)));

            List<SelectListItem> customerOptions = [new SelectListItem("-- Select customer --", "")];
            if (customersTask.Result.Data != null)
                customerOptions.AddRange(customersTask.Result.Data.Select(c => new SelectListItem(c, c)));

            List<SelectListItem> statusOptions = [new SelectListItem("-- Select status --", "")];
            if (statusesTask.Result.Data != null)
                statusOptions.AddRange(statusesTask.Result.Data.Select(s => new SelectListItem(s, s)));

            return new ProposedProjectViewModel
            {
                ProgramOptions = programOptions,
                CustomerOptions = customerOptions,
                StatusOptions = statusOptions
            };
        }
    }
}
