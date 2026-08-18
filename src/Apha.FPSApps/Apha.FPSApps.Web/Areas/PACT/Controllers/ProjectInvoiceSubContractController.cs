using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class ProjectInvoiceSubContractController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectSubContractService _subContractService;
        private readonly IProjectService _projectService;

        public ProjectInvoiceSubContractController(
            IMapper mapper,
            IProjectInvoiceService invoiceService,
            IProjectSubContractService subContractService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
            _subContractService = subContractService;
            _projectService = projectService;
        }

        // ── INDEX ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? parentProject)
        {
            var defaultRequest = new PaginationFilter<string>();

            var invoicesGridTask = BuildInvoicesGridAsync(defaultRequest, parentProject);
            var subContractsGridTask = BuildSubContractsGridAsync(defaultRequest, parentProject);
            var invoiceTotalTask = _invoiceService.GetTotalAmountAsync(parentProject);
            var subContractTotalTask = _subContractService.GetTotalAmountAsync(parentProject);

            await Task.WhenAll(invoicesGridTask, subContractsGridTask, invoiceTotalTask, subContractTotalTask);

            // Store the original parentProject on first visit, preserve it on subsequent visits
            string originalParentProject;
            if (TempData.Peek("OriginalParentProject") == null)
            {
                originalParentProject = parentProject ?? string.Empty;
                TempData["OriginalParentProject"] = originalParentProject;
            }
            else
            {
                originalParentProject = TempData.Peek("OriginalParentProject")?.ToString() ?? string.Empty;
                TempData.Keep("OriginalParentProject");
            }

            // Preserve TempData for return navigation and project code changes
            if (TempData.Peek("NavigationSource") != null)
            {
                TempData.Keep("NavigationSource");
            }
            if (TempData.Peek("PactOrigin") != null)
            {
                TempData.Keep("PactOrigin");
            }

            return View(new ProjectInvoiceSubContractViewModel
            {
                ParentProject = parentProject ?? string.Empty,
                OriginalParentProject = originalParentProject,
                FromPortfolio = TempData.Peek("PactOrigin") as string == "PortfolioMaintenance",
                InvoicesGrid = invoicesGridTask.Result,
                SubContractsGrid = subContractsGridTask.Result,
                TotalInvoiceAmount = invoiceTotalTask.Result.Data,
                TotalSubContractAmount = subContractTotalTask.Result.Data
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceTotalAmount(string? parentProject)
        {
            var response = await _invoiceService.GetTotalAmountAsync(parentProject);
            return Json(new { total = response.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetSubContractTotalAmount(string? parentProject)
        {
            var response = await _subContractService.GetTotalAmountAsync(parentProject);
            return Json(new { total = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> LoadInvoicesGrid(PaginationFilter<string> request, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gridConfig = await BuildInvoicesGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadSubContractsGrid(PaginationFilter<string> request, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gridConfig = await BuildSubContractsGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── INVOICE CRUD ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditInvoice", new ProjectInvoiceItem
                {
                    ProjectParent = parentProject ?? string.Empty
                });
            }

            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            return PartialView("_AddEditInvoice", _mapper.Map<ProjectInvoiceItem>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody] ProjectInvoiceItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            // Strip the JSON-path prefix ("$.Month" → "Month") produced
                            // when System.Text.Json fails to deserialise a property value.
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<ProjectInvoiceDto>(model);
            ApiResponseDto<ProjectInvoiceDto> result;
            string successMsg;

            if (model.InvoiceCounter == 0)
            {
                result = await _invoiceService.CreateAsync(dto);
                successMsg = "Invoice saved successfully.";
            }
            else
            {
                result = await _invoiceService.UpdateAsync(model.InvoiceCounter, dto);
                successMsg = "Invoice updated successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to create project job code.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _invoiceService.DeleteAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete invoice." });
        }

        // ── SUB-CONTRACT CRUD ─────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetSubContract(int id, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditSubContract", new ProjectSubContractItem
                {
                    Project = parentProject
                });
            }

            var result = await _subContractService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            return PartialView("_AddEditSubContract", _mapper.Map<ProjectSubContractItem>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubContract([FromBody] ProjectSubContractItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            // Strip the JSON-path prefix ("$.Month" → "Month") produced
                            // when System.Text.Json fails to deserialise a property value.
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<ProjectSubContractDto>(model);
            ApiResponseDto<ProjectSubContractDto> result;
            string successMsg;

            if (model.SubContCounter == 0)
            {
                result = await _subContractService.CreateAsync(dto);
                successMsg = "Sub-contract saved successfully.";
            }
            else
            {
                result = await _subContractService.UpdateAsync(model.SubContCounter, dto);
                successMsg = "Sub-contract updated successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to create project job code.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSubContract(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subContractService.DeleteAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete sub-contract." });
        }

        // ── PRIVATE GRID BUILDERS ─────────────────────────────────────────────

        private async Task<DataGridConfig<ProjectInvoiceItem>> BuildInvoicesGridAsync(
            PaginationFilter<string> request, string? parentProject)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _invoiceService.GetPagedProjectInvoicesAsync(query, parentProject);

            var items = response.Data != null
                ? _mapper.Map<List<ProjectInvoiceItem>>(response.Data)
                : new List<ProjectInvoiceItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProjectInvoiceItem>
            {
                GridId = "invoicesGrid",
                Title = "Invoices",
                KeyProperty = "InvoiceCounter",
                AddFunction = "addInvoice",
                EditFunction = "editInvoice",
                DeleteFunction = "deleteInvoice",
                BindGridUrl = string.IsNullOrEmpty(parentProject)
                    ? "/PACT/ProjectInvoiceSubContract/LoadInvoicesGrid"
                    : $"/PACT/ProjectInvoiceSubContract/LoadInvoicesGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectInvoiceItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<ProjectSubContractItem>> BuildSubContractsGridAsync(
            PaginationFilter<string> request, string? parentProject)
            {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _subContractService.GetPagedProjectSubContractsAsync(query, parentProject);

            var items = response.Data != null
                ? _mapper.Map<List<ProjectSubContractItem>>(response.Data)
                : new List<ProjectSubContractItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProjectSubContractItem>
            {
                GridId = "subContractsGrid",
                Title = "Sub-Contracts",
                KeyProperty = "SubContCounter",
                AddFunction = "addSubContract",
                EditFunction = "editSubContract",
                DeleteFunction = "deleteSubContract",
                BindGridUrl = string.IsNullOrEmpty(parentProject)
                    ? "/PACT/ProjectInvoiceSubContract/LoadSubContractsGrid"
                    : $"/PACT/ProjectInvoiceSubContract/LoadSubContractsGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectSubContractItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task PopulateProjectsViewBagAsync()
        {
            var result = await _projectService.GetAllPactProjectsAsync();
            ViewBag.Projects = (result.Success && result.Data != null)
                ? result.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text  = p.ParentProject
                    })
                    .ToList()
                : new List<SelectListItem>();
        }
    }
}
