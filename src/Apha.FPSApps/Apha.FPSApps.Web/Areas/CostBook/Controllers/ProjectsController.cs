using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class ProjectsController : Controller
    {
        private readonly ICostBookProjectService _projectService;
        private readonly ICostBookCustomerService _customerService;
        private readonly ICostBookDiseaseService _diseaseService;
        private readonly ICostBookProgramService _programService;
        private readonly ICostBookStaffService _staffService;
        private readonly ICostBookContractService _contractService;
     
        private readonly IMapper _mapper;

        public ProjectsController(
            ICostBookProjectService projectService,
            ICostBookCustomerService customerService,
            ICostBookDiseaseService diseaseService,
            ICostBookProgramService programService,
            ICostBookStaffService staffService,
            ICostBookContractService contractService,
            IMapper mapper)
        {
            _projectService = projectService;
            _customerService = customerService;
            _diseaseService = diseaseService;
            _programService = programService;
            _staffService = staffService;
            _contractService = contractService;
            _mapper = mapper;
        }

        // GET: /CostBook/Projects - Form OnLoad/OnOpen logic preserved with FPS DataGrid pattern
        public async Task<IActionResult> Index(string? searchTerm, int selectedYear = 0, int recordsPerPage = 5, int currentPage = 1, string? contractFilter = null, string? submittedByFilter = null)
        {
           
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };

            var projectGridConfig = await GetProjectGridConfigAsync(defaultRequest);

            // Create hybrid view model that supports both patterns
            var viewModel = new ProjectViewModel
            {
                ProjectGrid = projectGridConfig,
                // Preserve original properties for backward compatibility
                SearchTerm = searchTerm ?? string.Empty,
                SelectedYear = selectedYear,
                RecordsPerPage = recordsPerPage,
                CurrentPage = currentPage
            };

            // MS Access RowSource equivalent for fixed dropdowns - PRESERVED
            PopulateViewModelOptions(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadProjectGrid(PaginationFilter<string> request)
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

            var projectGridConfig = await GetProjectGridConfigAsync(request);
            return PartialView("_DataGrid", projectGridConfig);
        }

        private async Task<DataGridConfig<ProjectItemViewModel>> GetProjectGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);          

            try
            {
                var projectPagedData = await _projectService.GetFilteredProjectsAsync(queryParameters);

                List<ProjectItemViewModel> projectItems = new List<ProjectItemViewModel>();
                if (projectPagedData.Success && projectPagedData.Data != null)
                {
                    projectItems = _mapper.Map<List<ProjectItemViewModel>>(projectPagedData.Data.ToList());
                }
                else
                {
                    // Handle errors in MS Access style - log but don't crash
                    if (projectPagedData.Errors != null)
                    {
                        foreach (var error in projectPagedData.Errors)
                        {
                            // Could log these errors or handle them as needed
                            Console.WriteLine($"Project data error: {error.Message}");
                        }
                    }
                }

                PaginationModel paginationModel = projectPagedData.Pagination == null ? new PaginationModel() : _mapper.Map<PaginationModel>(projectPagedData.Pagination);
                paginationModel.SortColumn = request.SortBy;
                paginationModel.SortDirection = request.Descending;

                return new DataGridConfig<ProjectItemViewModel>
                {
                    GridId = "projectGrid",
                    Title = "Choose Existing Project",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    KeyProperty = "ProjectId",
                    AllowAdd = false,
                    AllowDelete = false,   
                    AllowEdit = false,
                    AllowView = true,                   
                    ViewFunction = "viewProject",
                                        
                    BindGridUrl = "/CostBook/Projects/LoadProjectGrid",
                    Data = projectItems,
                    Columns = GridDataProvider.GetColumnsDefination<ProjectItemViewModel>(null),
                    Pagination = paginationModel,
                    CurrentFilters = filterDict
                };
            }
            catch 
            {
                // MS Access style error handling - return empty grid instead of crashing
                return new DataGridConfig<ProjectItemViewModel>
                {
                    GridId = "projectGrid",
                    Title = "Project List",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    KeyProperty = "ProjectCode",
                    Data = new List<ProjectItemViewModel>(),
                    Columns = GridDataProvider.GetColumnsDefination<ProjectItemViewModel>(null),
                    Pagination = new PaginationModel(),
                    CurrentFilters = filterDict
                };
            }
        }



        // GET: /CostBook/Projects/Details/5 - MS Access OnCurrent equivalent
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var decodedId = HttpUtility.UrlDecode(id);
            var response = await _projectService.GetProjectByIdAsync(decodedId);

            if (!response.Success || response.Data == null)
                return NotFound();

            var viewModel = _mapper.Map<ProjectDetailViewModel>(response.Data);
            return View(viewModel);
        }

        // GET: /CostBook/Projects/Create - MS Access Form OnOpen for new record
        public async Task<IActionResult> Create()
        {
            var viewModel = new ProjectCreateEditViewModel
            {
                ProjectId = string.Empty, // Will be set by user
                Projecttitle = string.Empty, // Will be set by user
                ContractNumber = string.Empty,
                Startdate = DateOnly.FromDateTime(DateTime.Today),// DateTime.Today → DateOnly?
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }

        // POST: /CostBook/Projects/Create - MS Access BeforeUpdate validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var projectDto = _mapper.Map<ProjectDto>(viewModel);

                // MS Access ValidationRule equivalent
                if (await ValidateProjectBusinessRules(projectDto))
                {
                    var response = await _projectService.AddProjectAsync(projectDto);

                    if (response.Success && response.Data != null)
                    {
                        TempData["Success"] = "Project created successfully!";
                        return RedirectToAction(nameof(Details), new { id = response.Data.ProjectId });
                    }

                    foreach (var error in response.Errors ?? new List<ApiErrorDto>())
                    {
                        ModelState.AddModelError("", error.Message ?? "Unknown error");
                    }
                }
            }

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

       

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var decodedId = HttpUtility.UrlDecode(id);

            var editResponse = await _projectService.GetProjectByIdAsync(decodedId);

            if (!editResponse.Success || editResponse.Data == null)
                return NotFound();

            // ✅ FIXED
            var viewModel = _mapper.Map<ProjectCreateEditViewModel>(editResponse.Data);

            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }

     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProjectCreateEditViewModel viewModel)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var decodedId = HttpUtility.UrlDecode(id);

            if (decodedId != viewModel.ProjectId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var projectDto = _mapper.Map<ProjectDto>(viewModel);

                // ✅ Business validation
                if (await ValidateProjectBusinessRules(projectDto))
                {
                    var response = await _projectService.UpdateProjectAsync(decodedId, projectDto);

                    if (response.Success)
                    {
                        TempData["Success"] = "Project updated successfully!";
                        return RedirectToAction(nameof(Details), new { id = decodedId });
                    }

                    // ✅ API errors
                    foreach (var error in response.Errors ?? new List<ApiErrorDto>())
                    {
                        ModelState.AddModelError("", error.Message ?? "Unknown error");
                    }
                }
            }

            // ❗ IMPORTANT: Re-populate dropdowns when returning view
            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }

        // GET: /CostBook/Projects/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var decodedId = HttpUtility.UrlDecode(id);
            var response = await _projectService.GetProjectByIdAsync(decodedId);

            if (!response.Success || response.Data == null)
                return NotFound();

            var viewModel = _mapper.Map<ProjectDetailViewModel>(response.Data);
            return View(viewModel);
        }

        // POST: /CostBook/Projects/Delete/5 - MS Access OnClick Delete button
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            //var response = await _projectService.DeleteProjectAsync(id);
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var decodedId = HttpUtility.UrlDecode(id);
            var response = await _projectService.DeleteProjectAsync(decodedId);

            if (response.Success)
            {
                TempData["Success"] = "Project deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete project.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Control-level events (MS Access OnClick, AfterUpdate equivalents)

        // MS Access AfterUpdate for dropdown - Control event handler
        [HttpPost]
        public async Task<JsonResult> OnCustomerChange(string customerName)
        {
            // Business logic when customer dropdown changes
            var programs = await _programService.GetAllProgramsAsync();

            var customerPrograms = programs.Success && programs.Data != null
                ? programs.Data.Where(p => p.Customer == customerName)
                               .Select(p => new { value = p.ProgramNo, text = p.ProgramName })
                               .Cast<object>()
                               .ToList()
                : new List<object>();

            return Json(new { programs = customerPrograms });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Copy(string id, string newId)
        {
            try
            {
                // URL decode the input newId if it's provided
                if (!string.IsNullOrEmpty(newId))
                {
                    newId = HttpUtility.UrlDecode(newId);
                }

                // If newId is provided, check if it already exists
                if (!string.IsNullOrEmpty(newId))
                {
                    var existingProjectResponse = await _projectService.GetProjectByIdAsync(newId);

                    // If the project with newId already exists, generate a new sequential ID
                    if (existingProjectResponse.Success && existingProjectResponse.Data != null)
                    {
                        var nextIdResponse = await _projectService.GetNextProjectNumberAsync(newId);
                        if (nextIdResponse.Success && !string.IsNullOrEmpty(nextIdResponse.Data))
                        {
                            newId = nextIdResponse.Data;
                        }
                        else
                        {
                            return Json(new { success = false, errors = new[] { "Failed to generate new project ID" } });
                        }
                    }
                }
                else
                {
                    // If newId is not provided, generate one based on the oldId (same logic as API)
                    var nextIdResponse = await _projectService.GetNextProjectNumberAsync(id);
                    if (nextIdResponse.Success && !string.IsNullOrEmpty(nextIdResponse.Data))
                    {
                        newId = nextIdResponse.Data;
                    }
                    else
                    {
                        return Json(new { success = false, errors = new[] { "Failed to generate new project ID" } });
                    }
                }

                var response = await _projectService.CopyProjectAsync(id, newId);

                if (response.Success)
                {
                    return Json(new
                    {
                        success = true,
                        projectId = response.Data?.ProjectId,
                        message = "Project copied successfully!",
                        generatedId = newId  // Return the generated ID for user feedback
                    });
                }

                return Json(new { success = false, errors = response.Errors?.Select(e => e.Message) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new[] { $"Error copying project: {ex.Message}" } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Recost(string id)
        {
            var response = await _projectService.RecostProjectAsync(id);
            return Json(new { success = response.Success && response.Data });
        }

        // Helper methods for business logic (MS Access VBA equivalent)
        private async Task<bool> ValidateProjectBusinessRules(ProjectDto project)
        {
            // MS Access ValidationRule equivalent
            //if (project.EndDate.HasValue && project.EndDate < project.Startdate)
            if (project.EndDate.HasValue && project.Startdate.HasValue &&
    DateOnly.FromDateTime(project.EndDate.Value) < project.Startdate.Value)

            {
                ModelState.AddModelError("EndDate", "End date cannot be earlier than start date");
                return false;
            }

            if (project.BudgetAmount.HasValue && project.BudgetAmount < 0)
            {
                ModelState.AddModelError("BudgetAmount", "Budget amount cannot be negative");
                return false;
            }

            if (project.ActualCost.HasValue && project.ActualCost < 0)
            {
                ModelState.AddModelError("ActualCost", "Actual cost cannot be negative");
                return false;
            }

            return true;
        }

        private void PopulateViewModelOptions(ProjectViewModel viewModel)
        {
            // MS Access RowSource equivalent for fixed dropdowns
            viewModel.YearOptions = new List<SelectListItem>
            {
                new() { Value = "2024", Text = "2024" },
                new() { Value = "2025", Text = "2025", Selected = viewModel.SelectedYear == 2025 },
                new() { Value = "2026", Text = "2026" },
                new() { Value = "2027", Text = "2027" }
            };

            viewModel.RecordsPerPageOptions = new List<SelectListItem>
            {
                new() { Value = "5", Text = "5", Selected = viewModel.RecordsPerPage == 5 },
                new() { Value = "10", Text = "10", Selected = viewModel.RecordsPerPage == 10 },
                new() { Value = "15", Text = "15" , Selected = viewModel.RecordsPerPage == 15 },
                new() { Value = "20", Text = "20" , Selected = viewModel.RecordsPerPage == 20},
                new() { Value = "25", Text = "25" , Selected = viewModel.RecordsPerPage == 25},
                new() { Value = "30", Text = "30",Selected = viewModel.RecordsPerPage == 30}
            };
        }

        private async Task PopulateDropdownsAsync(ProjectCreateEditViewModel viewModel)
        {
            var programsResponse = await _programService.GetAllProgramsAsync();
            var customersResponse = await _customerService.GetAllCustomersAsync();
            var diseasesResponse = await _diseaseService.GetAllDiseasesAsync();
            var staffResponse = await _staffService.GetAllStaffAsync();
            var contractsResponse = await _contractService.GetAllContractNumbersAsync();

            // ✅ Programs with selected value
            viewModel.AvailablePrograms = programsResponse.Data?
                .Select(p => new SelectListItem 
                { 
                    Value = p.ProgramNo, 
                    Text = p.ProgramNo,
                    Selected = p.ProgramNo == viewModel.Programme
                })
                .ToList() ?? new List<SelectListItem>();

            // ✅ Customers with selected value
            viewModel.AvailableCustomers = customersResponse.Data?
                .Select(c => new SelectListItem 
                { 
                    Value = c.CustomerName, 
                    Text = c.CustomerName,
                    Selected = c.CustomerName == viewModel.CustomerName
                })
                .ToList() ?? new List<SelectListItem>();

            // ✅ Diseases with selected value
            viewModel.AvailableDiseases = diseasesResponse.Data?
                .Select(d => new SelectListItem 
                { 
                    Value = d.DiseaseName, 
                    Text = d.DiseaseName,
                    Selected = d.DiseaseName == viewModel.Disease
                })
                .ToList() ?? new List<SelectListItem>();

            // ✅ Staff with selected value
            viewModel.AvailableStaff = staffResponse.Data?
                .Select(s => new SelectListItem 
                { 
                    Value = s.Name, 
                    Text = s.Name,
                    Selected = s.Name == viewModel.PreparedBy
                })
                .ToList() ?? new List<SelectListItem>();

            // ✅ Contracts with selected value
            viewModel.AvailableContracts = contractsResponse.Data?
                .Select(c => new SelectListItem 
                { 
                    Value = c.ContractNumber, 
                    Text = c.ContractNumber,
                    Selected = c.ContractNumber == viewModel.ContractNumber
                })
                .ToList() ?? new List<SelectListItem>();

        }        
    }
}
