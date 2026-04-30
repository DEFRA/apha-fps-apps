using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
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

        private static readonly List<(string Value, string Text)> FinancialYearOptions =
        [
            ("-1", "Financial Years"),
            ("0",  "Project Years")
        ];

        private static readonly List<(string Value, string Text)> DefraProjectOptions =
        [
            ("-1", "Yes"),
            ("0",  "No")
        ];

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
            var viewModel = new ProjectViewModel
            {
                ProjectGrid = projectGridConfig,
                SearchTerm = searchTerm ?? string.Empty,
                SelectedYear = selectedYear,
                RecordsPerPage = recordsPerPage,
                CurrentPage = currentPage
            };
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
                    if (projectPagedData.Errors != null)
                    {
                        foreach (var error in projectPagedData.Errors)
                        {                            
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

       
        public async Task<IActionResult> Create()
        {
            var viewModel = new ProjectCreateEditViewModel
            {
                ProjectId = string.Empty, 
                ProjectTitle = string.Empty, 
                ContractNumber = string.Empty,
                StartDate = null,
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateEditViewModel viewModel)
        {
            // Handle unchecked checkbox: null → 0, checked → -1
            viewModel.Inflation ??= 0;
            //ModelState.Remove(nameof(viewModel.ProjectId));

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
                        return RedirectToAction(nameof(Edit), new { id = response.Data.ProjectId });
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
                throw new ArgumentException("Project ID is required", nameof(id));

            var decodedId = HttpUtility.UrlDecode(id);

            var editResponse = await _projectService.GetProjectByIdAsync(decodedId);

            if (!editResponse.Success || editResponse.Data == null)
                throw new KeyNotFoundException($"Project with ID '{decodedId}' not found");

         
            var viewModel = _mapper.Map<ProjectCreateEditViewModel>(editResponse.Data);

            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProjectCreateEditViewModel viewModel)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Project ID is required", nameof(id));

            var decodedId = HttpUtility.UrlDecode(id);

            if (decodedId != viewModel.ProjectId)
                throw new ArgumentException("Project ID mismatch", nameof(id));

            // Handle unchecked checkbox: null → 0, checked → -1
            viewModel.Inflation ??= 0;

            if (ModelState.IsValid)
            {
                var projectDto = _mapper.Map<ProjectDto>(viewModel);
                var response = await _projectService.UpdateProjectAsync(decodedId, projectDto);

                if (response.Success)
                {
                    TempData["Success"] = "Project updated successfully!";
                    return RedirectToAction(nameof(Edit), new { id = decodedId });
                }

                foreach (var error in response.Errors ?? new List<ApiErrorDto>())
                    ModelState.AddModelError(string.Empty, error.Message ?? "Unknown error");
            }

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }
        

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Project ID is required." });

            var decodedId = HttpUtility.UrlDecode(id);
            var response = await _projectService.DeleteProjectAsync(decodedId);

            if (!response.Success)
            {
                var errorMessage = response.Errors?.Count > 0
                    ? string.Join(", ", response.Errors.Select(e => e.Message))
                    : "Project deletion failed. Please try again.";

                return Json(new
                {
                    success = false,
                    message = errorMessage,
                    errors = response.Errors?.Select(e => e.Message)
                });
            }

            return Json(new { success = true, message = "Project deleted successfully!" });
        }

        // ── Copy (AJAX — returns JSON, matches FPS pattern) ──────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Copy(string sourceProjectId)
        {
            var nextIdResponse = await _projectService.GetNextProjectNumberAsync(sourceProjectId);

            if (!nextIdResponse.Success || string.IsNullOrEmpty(nextIdResponse.Data))
                return Json(new { success = false, message = "Failed to generate new project ID." });

            var newProjectId = nextIdResponse.Data;
            var response = await _projectService.CopyProjectAsync(sourceProjectId, newProjectId);

            if (response.Success)
                return Json(new
                {
                    success = true,
                    message = "Project copied successfully!",
                    projectId = response.Data?.ProjectId,
                    generatedId = newProjectId
                });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to copy project.",
                errors = response.Errors?.Select(e => e.Message)
            });
        }

        // ── Recost (AJAX — returns JSON) ──────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Recost(string id)
        {
            var response = await _projectService.RecostProjectAsync(id);
            return Json(new
            {
                success = response.Success && response.Data,
                message = response.Success ? "Project recosted successfully." : "Recost failed."
            });
        }
        

        [HttpGet]
        public async Task<IActionResult> GetNextProjectId()
        {
            var response = await _projectService.GetNextProjectNumberAsync(null);

            if (response.Success && response.Data != null)
                return Json(new { success = true, projectId = response.Data });

            return Json(new { success = false, projectId = (string?)null });
        }

        

        [HttpPost]
        public async Task<JsonResult> OnCustomerChange(string customerName)
        {
            var programs = await _programService.GetAllProgramsAsync();

            var customerPrograms = programs.Success && programs.Data != null
                ? programs.Data
                    .Where(p => p.Customer == customerName)
                    .Select(p => new { value = p.ProgramNo, text = p.ProgramName })
                    .Cast<object>()
                    .ToList()
                : new List<object>();

            return Json(new { programs = customerPrograms });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<bool> ValidateProjectBusinessRules(ProjectDto project)
        {
            if (project.EndDate.HasValue && project.StartDate.HasValue &&
                DateOnly.FromDateTime(project.EndDate.Value) < project.StartDate.Value)
            {
                ModelState.AddModelError("EndDate", "End date cannot be earlier than start date.");
                return false;
            }

            if (project.BudgetAmount.HasValue && project.BudgetAmount < 0)
            {
                ModelState.AddModelError("BudgetAmount", "Budget amount cannot be negative.");
                return false;
            }

            if (project.ActualCost.HasValue && project.ActualCost < 0)
            {
                ModelState.AddModelError("ActualCost", "Actual cost cannot be negative.");
                return false;
            }

            return true;
        }

        private  static void PopulateViewModelOptions(ProjectViewModel viewModel)
        {
            viewModel.RecordsPerPageOptions = new List<SelectListItem>
            {
                new() { Value = "5",  Text = "5",  Selected = viewModel.RecordsPerPage == 5  },
                new() { Value = "10", Text = "10", Selected = viewModel.RecordsPerPage == 10 },
                new() { Value = "15", Text = "15", Selected = viewModel.RecordsPerPage == 15 },
                new() { Value = "20", Text = "20", Selected = viewModel.RecordsPerPage == 20 },
                new() { Value = "25", Text = "25", Selected = viewModel.RecordsPerPage == 25 },
                new() { Value = "30", Text = "30", Selected = viewModel.RecordsPerPage == 30 }
            };
        }

        private async Task PopulateDropdownsAsync(ProjectCreateEditViewModel viewModel)
        {
            var programsResponse = await _programService.GetAllProgramsAsync();
            var customersResponse = await _customerService.GetAllCustomersAsync();
            var diseasesResponse = await _diseaseService.GetAllDiseasesAsync();
            var staffResponse = await _staffService.GetAllStaffAsync();
            

            // Programs with selected value
            var programsList = programsResponse.Data?
                .Select(p => new SelectListItem
                {
                    Value = p.ProgramNo,
                    Text = p.ProgramNo,
                    Selected = p.ProgramNo == viewModel.Programme
                })
                .ToList() ?? new List<SelectListItem>();

            // If Programme is set but not in the programs list, add it to preserve the value
            if (!string.IsNullOrEmpty(viewModel.Programme) &&
                !programsList.Any(p => p.Value == viewModel.Programme))
            {
                programsList.Insert(0, new SelectListItem
                {
                    Value = viewModel.Programme,
                    Text = viewModel.Programme,
                    Selected = true
                });
            }

            viewModel.AvailablePrograms = programsList;

            // Customers with selected value
            var customersList = customersResponse.Data?
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerName,
                    Text = c.CustomerName,
                    Selected = c.CustomerName == viewModel.CustomerName
                })
                .ToList() ?? new List<SelectListItem>();

            // If CustomerName is set but not in the customers list, add it to preserve the value
            if (!string.IsNullOrEmpty(viewModel.CustomerName) &&
                !customersList.Any(c => c.Value == viewModel.CustomerName))
            {
                customersList.Insert(0, new SelectListItem
                {
                    Value = viewModel.CustomerName,
                    Text = viewModel.CustomerName,
                    Selected = true
                });
            }

            viewModel.AvailableCustomers = customersList;

            // Diseases with selected value
            var diseasesList = diseasesResponse.Data?
                .Select(d => new SelectListItem
                {
                    Value = d.DiseaseName,
                    Text = d.DiseaseName,
                    Selected = d.DiseaseName == viewModel.Disease
                })
                .ToList() ?? new List<SelectListItem>();

            // If Disease is set but not in the diseases list, add it to preserve the value
            if (!string.IsNullOrEmpty(viewModel.Disease) &&
                !diseasesList.Any(d => d.Value == viewModel.Disease))
            {
                diseasesList.Insert(0, new SelectListItem
                {
                    Value = viewModel.Disease,
                    Text = viewModel.Disease,
                    Selected = true
                });
            }

            viewModel.AvailableDiseases = diseasesList;

            // Staff with selected value
            var staffList = staffResponse.Data?
               .Select(s => new SelectListItem
               {
                   Value = s.Name,
                   Text = s.Name,
                   Selected = s.Name == viewModel.PreparedBy
               })
               .ToList() ?? new List<SelectListItem>();

            
            if (!string.IsNullOrEmpty(viewModel.PreparedBy) &&
                !staffList.Any(s => s.Value == viewModel.PreparedBy))
            {
                staffList.Insert(0, new SelectListItem
                {
                    Value = viewModel.PreparedBy,
                    Text = viewModel.PreparedBy,
                    Selected = true
                });
            }

            viewModel.AvailableStaff = staffList;            

            // Available Financial Years with selected value
            var selectedFinancialYear = viewModel.FinancialYears?.ToString() ?? "-1";
            viewModel.AvailableFinancialYears = FinancialYearOptions
                .Select(item => new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text,
                    Selected = item.Value == selectedFinancialYear
                })
                .ToList();

            // Available Defra Project Options with selected value
            var selectedDefraProject = viewModel.IsDefraProject?.ToString() ?? string.Empty;  // ← was ?? "-1"
            viewModel.AvailableDefraProjectOptions = DefraProjectOptions
                .Select(item => new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text,
                    Selected = item.Value == selectedDefraProject
                })
                .ToList();
        }
      
    }
}
