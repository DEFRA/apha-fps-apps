using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class TotalBusinessOverheadsMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMaintTotalBusinessOverheadsService _service;

        public TotalBusinessOverheadsMaintenanceController(IMapper mapper, IMaintTotalBusinessOverheadsService service)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IActionResult> Index()
        {
            var result = await _service.GetAsync();
            var viewModel = new TotalBusinessOverheadsViewModel();

            if (result.Success && result.Data != null)
            {
                viewModel = _mapper.Map<TotalBusinessOverheadsViewModel>(result.Data);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TotalBusinessOverheadsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var dto = _mapper.Map<TotalBusinessOverheadsDto>(model);
            var result = await _service.UpdateAsync(dto);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Total Business Overheads saved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to save Total Business Overheads.";
            }

            return RedirectToAction("Index");
        }
    }
}
