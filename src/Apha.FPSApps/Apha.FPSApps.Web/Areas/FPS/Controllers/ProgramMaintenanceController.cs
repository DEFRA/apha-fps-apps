using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{

    [Area("FPS")]
    [AllowAnonymous]
    public class ProgramMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;

        public ProgramMaintenanceController(IMapper mapper, IProgramService programService)
        {
            _mapper = mapper;
            _programService = programService;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _programService.GetAllProgramsAsync(new QueryParameters<string>());

            var programItems = new List<ProgramViewModel>();
            if (response.Data != null)
            {
                programItems = _mapper.Map<List<ProgramViewModel>>(response.Data.ToList());
            }
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(response.Pagination);

            var programGridConfig = new DataGridConfig<ProgramViewModel>
            {
                GridId = "programGrid",
                Title = "Programs",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "ProgramNo",
                AddFunction = "addProgram",
                EditFunction = "editProgram",
                DeleteFunction = "deleteProgram",
                ExtraFilterMethod = "getProgramExtraFilters",
                BindGridUrl = "/FPS/ProgramMaintenance/LoadProgramGrid",
                Data = programItems,
                Columns = GridDataProvider.GetColumnsDefination<ProgramViewModel>(null),
                Pagination = paginationModel
            };

            //var viewModel = _mapper.Map<List<ProgramViewModel>>(response.Data);

            return View(programGridConfig);
        }

        // GET: Paginated list
        public async Task<IActionResult> List(QueryParameters<string> query)
        {
            var response = await _programService.GetAllProgramsAsync(query);
            var viewModel = _mapper.Map<List<ProgramViewModel>>(response.Data?.ToList());
            ViewBag.Pagination = response.Pagination;
            return View(viewModel);
        }

        // GET: Details
        public async Task<IActionResult> Details(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return NotFound();
            var viewModel = _mapper.Map<ProgramViewModel>(response.Data);
            return View(viewModel);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View(new ProgramViewModel
            {
                ProgramNo = string.Empty, // Initialize with default or placeholder value
                ProgramName = string.Empty, // Initialize with default or placeholder value
                Directorate = string.Empty // Initialize with default or placeholder value
            });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgramViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = _mapper.Map<ProgramDto>(model);
            var response = await _programService.AddProgramAsync(dto);
            if (response.Success)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Failed to add program.");
            return View(model);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return NotFound();
            var viewModel = _mapper.Map<ProgramViewModel>(response.Data);
            return View(viewModel);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProgramViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = _mapper.Map<ProgramDto>(model);
            var response = await _programService.UpdateProgramAsync(dto);
            if (response.Success)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Failed to update program.");
            return View(model);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return NotFound();
            var viewModel = _mapper.Map<ProgramViewModel>(response.Data);
            return View(viewModel);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string programNo)
        {
            var response = await _programService.DeleteProgramAsync(programNo);
            if (response.Success)
                return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Failed to delete program.");
            return RedirectToAction(nameof(Delete), new { programNo });
        }

        // GET: Directorates
        public async Task<IActionResult> Directorates()
        {
            var response = await _programService.GetAllDirectoratesAsync();
            return View(response.Data);
        }
    }
}
