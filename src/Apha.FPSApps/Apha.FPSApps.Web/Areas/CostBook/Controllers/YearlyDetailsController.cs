using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers;

[Area("CostBook")]
[Authorize(Roles = "CostbookAdmin,CostbookUser")]
[AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
public class YearlyDetailsController : Controller
{
    private readonly ICostBookYearlyDetailsService _service;
    private readonly IMapper _mapper;

    public YearlyDetailsController(ICostBookYearlyDetailsService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // ── INDEX ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string projectId, int selectedYear = 0)
    {
        var decodedProjectId = HttpUtility.UrlDecode(projectId);

        var headerResponse = await _service.GetProjectHeaderAsync(decodedProjectId);
        if (!headerResponse.Success || headerResponse.Data is null)
            return RedirectToAction("Index", "Projects");

        var header = headerResponse.Data;
        var isDefra = header.IsDefraProject == 1;

        var yearsResponse = await _service.GetProjectYearsAsync(decodedProjectId);
        var years = yearsResponse.Success && yearsResponse.Data != null
            ? yearsResponse.Data.Select(y => y.YearValue).ToList()
            : new List<int>();

        if (selectedYear == 0)
            selectedYear = years.FirstOrDefault();

        var viewModel = new YearlyDetailsViewModel
        {
            Code = header.ProjectId,
            ProjectTitle = header.ProjectTitle,
            StartDate = header.StartDate,
            FinancialYears = header.FinancialYears,
            Inflation = header.Inflation,
            IsDefraProject = header.IsDefraProject,
            EuroConvRate = header.EuroConvRate,
            Programme = header.Programme,
            SelectedYear = selectedYear,
            ProjectYears = years
        };

        await PopulateDropdownsAsync(viewModel, isDefra);

        if (selectedYear > 0)
        {
            viewModel.StaffGrid = await BuildStaffGridAsync(decodedProjectId, selectedYear);
            viewModel.TestGrid = await BuildTestGridAsync(decodedProjectId, selectedYear);
            viewModel.AnimalGrid = await BuildAnimalGridAsync(decodedProjectId, selectedYear);
            viewModel.AdditionalCostGrid = await BuildAdditionalCostGridAsync(decodedProjectId, selectedYear);

            var ratesResponse = await _service.GetProjectYearsAsync(decodedProjectId);
            if (ratesResponse.Success && ratesResponse.Data != null)
                viewModel.YearRates = _mapper.Map<List<ProjectYearRateItem>>(ratesResponse.Data);
        }

        return View(viewModel);
    }

    // ── GRID LOADERS ──────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var gridConfig = await BuildStaffGridAsync(HttpUtility.UrlDecode(projectId), year, query);
        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadTestGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var gridConfig = await BuildTestGridAsync(HttpUtility.UrlDecode(projectId), year);
        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadAnimalGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var gridConfig = await BuildAnimalGridAsync(HttpUtility.UrlDecode(projectId), year);
        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadAdditionalCostGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var gridConfig = await BuildAdditionalCostGridAsync(HttpUtility.UrlDecode(projectId), year);
        return PartialView("_DataGrid", gridConfig);
    }

    // ── ADD PROJECT YEAR (btnAddProjectYear — fnAddProjectYear BAS) ───────

    [HttpPost]
    public async Task<IActionResult> AddProjectYear(string projectId, int year)
    {
        var response = await _service.AddProjectYearAsync(HttpUtility.UrlDecode(projectId), year);
        if (!response.Success)
            return Json(new { success = false, message = "Failed to add project year." });
        return Json(new { success = true, year = response.Data?.YearValue });
    }

    // ── STAFF CRUD ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CreateStaff(string projectId, int year, bool isDefra)
    {
        var payRates = await GetPayRateOptionsAsync(isDefra);
        return PartialView("_AddEditStaffRequirement", new StaffRequirementItem { WgGrade = string.Empty });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(string projectId, int year, StaffRequirementItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditStaffRequirement", item);
        var dto = _mapper.Map<StaffRequirementDto>(item);
        var response = await _service.AddStaffRequirementAsync(HttpUtility.UrlDecode(projectId), year, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> EditStaff(string projectId, int year, int srIdentity, bool isDefra)
    {
        // Fetch full list (staff rows per project/year are few; pageSize 1000 covers all cases)
        var query = new QueryParameters<string> { Page = 1, PageSize = 1000 };
        var listResponse = await _service.GetStaffRequirementsAsync(
                               HttpUtility.UrlDecode(projectId), year, query);

        var row = listResponse.Data?.data?.FirstOrDefault(s => s.SrIdentity == srIdentity);
        if (row is null) return NotFound();

        return PartialView("_AddEditStaffRequirement", _mapper.Map<StaffRequirementItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditStaff(string projectId, int year, int srIdentity, StaffRequirementItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditStaffRequirement", item);
        var dto = _mapper.Map<StaffRequirementDto>(item);
        var response = await _service.UpdateStaffRequirementAsync(HttpUtility.UrlDecode(projectId), year, srIdentity, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStaff(string projectId, int year, int srIdentity)
    {
        var response = await _service.DeleteStaffRequirementAsync(HttpUtility.UrlDecode(projectId), year, srIdentity);
        return Json(new { success = response.Success && response.Data });
    }

    // ── TEST CRUD ─────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateTest(string projectId, int year)
        => PartialView("_AddEditTestRequirement", new TestRequirementItem { TestCode = string.Empty });

    [HttpPost]
    public async Task<IActionResult> CreateTest(string projectId, int year, TestRequirementItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditTestRequirement", item);
        var dto = _mapper.Map<TestRequirementDto>(item);
        var response = await _service.AddTestRequirementAsync(HttpUtility.UrlDecode(projectId), year, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> EditTest(string projectId, int year, string testCode)
    {
        var listResponse = await _service.GetTestRequirementsAsync(HttpUtility.UrlDecode(projectId), year);
        var row = listResponse.Data?.FirstOrDefault(t => t.TestCode == testCode);
        if (row is null) return NotFound();
        return PartialView("_AddEditTestRequirement", _mapper.Map<TestRequirementItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditTest(string projectId, int year, string testCode, TestRequirementItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditTestRequirement", item);
        var dto = _mapper.Map<TestRequirementDto>(item);
        var response = await _service.UpdateTestRequirementAsync(HttpUtility.UrlDecode(projectId), year, testCode, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteTest(string projectId, int year, string testCode)
    {
        var response = await _service.DeleteTestRequirementAsync(HttpUtility.UrlDecode(projectId), year, testCode);
        return Json(new { success = response.Success && response.Data });
    }

    // ── ANIMAL CRUD ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateAnimal(string projectId, int year)
        => PartialView("_AddEditAnimalRequirement", new AnimalRequirementItem { AnimalType = string.Empty });

    [HttpPost]
    public async Task<IActionResult> CreateAnimal(string projectId, int year, AnimalRequirementItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditAnimalRequirement", item);
        var dto = _mapper.Map<AnimalRequirementDto>(item);
        var response = await _service.AddAnimalRequirementAsync(HttpUtility.UrlDecode(projectId), year, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> EditAnimal(string projectId, int year, int arIdentity)
    {
        var listResponse = await _service.GetAnimalRequirementsAsync(HttpUtility.UrlDecode(projectId), year);
        var row = listResponse.Data?.FirstOrDefault(a => a.ArIdentity == arIdentity);
        if (row is null) return NotFound();
        return PartialView("_AddEditAnimalRequirement", _mapper.Map<AnimalRequirementItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditAnimal(string projectId, int year, int arIdentity, AnimalRequirementItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditAnimalRequirement", item);
        var dto = _mapper.Map<AnimalRequirementDto>(item);
        var response = await _service.UpdateAnimalRequirementAsync(HttpUtility.UrlDecode(projectId), year, arIdentity, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAnimal(string projectId, int year, int arIdentity)
    {
        var response = await _service.DeleteAnimalRequirementAsync(HttpUtility.UrlDecode(projectId), year, arIdentity);
        return Json(new { success = response.Success && response.Data });
    }

    // ── ADDITIONAL COST CRUD ──────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateAdditionalCost(string projectId, int year)
        => PartialView("_AddEditAdditionalCost", new AdditionalCostItem { Description = string.Empty, AccountCat = string.Empty });

    [HttpPost]
    public async Task<IActionResult> CreateAdditionalCost(string projectId, int year, AdditionalCostItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditAdditionalCost", item);
        var dto = _mapper.Map<AdditionalCostDto>(item);
        var response = await _service.AddAdditionalCostAsync(HttpUtility.UrlDecode(projectId), year, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> EditAdditionalCost(string projectId, int year, int acIdentity)
    {
        var listResponse = await _service.GetAdditionalCostsAsync(HttpUtility.UrlDecode(projectId), year);
        var row = listResponse.Data?.FirstOrDefault(ac => ac.AcIdentity == acIdentity);
        if (row is null) return NotFound();
        return PartialView("_AddEditAdditionalCost", _mapper.Map<AdditionalCostItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditAdditionalCost(string projectId, int year, int acIdentity, AdditionalCostItem item)
    {
        if (!ModelState.IsValid)
            return PartialView("_AddEditAdditionalCost", item);
        var dto = _mapper.Map<AdditionalCostDto>(item);
        var response = await _service.UpdateAdditionalCostAsync(HttpUtility.UrlDecode(projectId), year, acIdentity, dto);
        if (!response.Success)
            return Json(new { success = false });
        return Json(new { success = true });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAdditionalCost(string projectId, int year, int acIdentity)
    {
        var response = await _service.DeleteAdditionalCostAsync(HttpUtility.UrlDecode(projectId), year, acIdentity);
        return Json(new { success = response.Success && response.Data });
    }

    // ── MARKUP/PROFIT UPDATE ──────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> UpdateProjectYearRate(string projectId, int year, ProjectYearRateItem item)
    {
        var dto = _mapper.Map<ProjectYearDto>(item);
        var response = await _service.UpdateProjectYearAsync(HttpUtility.UrlDecode(projectId), year, dto);
        return Json(new { success = response.Success });
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────

    private async Task<DataGridConfig<StaffRequirementItem>> BuildStaffGridAsync(
        string projectId, int year, QueryParameters<string>? query = null)
    {
        query ??= new QueryParameters<string> { Page = 1, PageSize = 10 };

        var response = await _service.GetStaffRequirementsAsync(projectId, year, query);
        var pagedResult = response.Success ? response.Data : null;

        var data = pagedResult?.data != null
            ? _mapper.Map<List<StaffRequirementItem>>(pagedResult.data)
            : new List<StaffRequirementItem>();

        return new DataGridConfig<StaffRequirementItem>
        {
            GridId      = "staffGrid",
            Title       = "Staff",
            Data        = data,
            KeyProperty = nameof(StaffRequirementItem.SrIdentity),
            AllowAdd    = true,
            AllowEdit   = true,
            AllowDelete = true,
            AllowCopy   = false,
            ShowPagination = true,
            Pagination  = new PaginationModel
            {
                TotalRecords = pagedResult?.TotalCount ?? 0,
                PageNumber   = query.Page,
                PageSize     = query.PageSize,
                SortColumn   = query.SortBy,
                SortDirection = query.Descending
            },
            AddFunction  = $"openAddStaffModal('{projectId}', {year})",
            EditFunction = $"openEditStaffModal('{projectId}', {year})",
            DeleteFunction = $"deleteStaff('{projectId}', {year})",
            BindGridUrl  = Url.Action("LoadStaffGrid", new { projectId, year }) ?? string.Empty,
            Columns      = GridDataProvider.GetColumnsDefination<StaffRequirementItem>(null)
        };
    }

    private async Task<DataGridConfig<TestRequirementItem>> BuildTestGridAsync(string projectId, int year)
    {
        var response = await _service.GetTestRequirementsAsync(projectId, year);
        var data = response.Success && response.Data != null
            ? _mapper.Map<List<TestRequirementItem>>(response.Data)
            : new List<TestRequirementItem>();

        return new DataGridConfig<TestRequirementItem>
        {
            GridId = "testGrid",
            Title = "Tests",
            Data = data,
            KeyProperty = nameof(TestRequirementItem.TestCode),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            AddFunction = $"openAddTestModal('{projectId}', {year})",
            EditFunction = $"openEditTestModal('{projectId}', {year})",
            DeleteFunction = $"deleteTest('{projectId}', {year})",
            BindGridUrl = Url.Action("LoadTestGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<TestRequirementItem>(null)
        };
    }

    private async Task<DataGridConfig<AnimalRequirementItem>> BuildAnimalGridAsync(string projectId, int year)
    {
        var response = await _service.GetAnimalRequirementsAsync(projectId, year);
        var data = response.Success && response.Data != null
            ? _mapper.Map<List<AnimalRequirementItem>>(response.Data)
            : new List<AnimalRequirementItem>();

        return new DataGridConfig<AnimalRequirementItem>
        {
            GridId = "animalGrid",
            Title = "Animals",
            Data = data,
            KeyProperty = nameof(AnimalRequirementItem.ArIdentity),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            AddFunction = $"openAddAnimalModal('{projectId}', {year})",
            EditFunction = $"openEditAnimalModal('{projectId}', {year})",
            DeleteFunction = $"deleteAnimal('{projectId}', {year})",
            BindGridUrl = Url.Action("LoadAnimalGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<AnimalRequirementItem>(null)
        };
    }

    private async Task<DataGridConfig<AdditionalCostItem>> BuildAdditionalCostGridAsync(string projectId, int year)
    {
        var response = await _service.GetAdditionalCostsAsync(projectId, year);
        var data = response.Success && response.Data != null
            ? _mapper.Map<List<AdditionalCostItem>>(response.Data)
            : new List<AdditionalCostItem>();

        return new DataGridConfig<AdditionalCostItem>
        {
            GridId = "additionalCostGrid",
            Title = "Additional Costs",
            Data = data,
            KeyProperty = nameof(AdditionalCostItem.AcIdentity),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            AddFunction = $"openAddAdditionalCostModal('{projectId}', {year})",
            EditFunction = $"openEditAdditionalCostModal('{projectId}', {year})",
            DeleteFunction = $"deleteAdditionalCost('{projectId}', {year})",
            BindGridUrl = Url.Action("LoadAdditionalCostGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<AdditionalCostItem>(null)
        };
    }

    private async Task PopulateDropdownsAsync(YearlyDetailsViewModel viewModel, bool isDefra)
    {
        var payRates = await _service.GetPayRatesAsync(isDefra);
        viewModel.WgGradeOptions = payRates.Success && payRates.Data != null
            ? payRates.Data.Select(p => new SelectListItem(p.WgGrade, p.WgGrade)).ToList()
            : new List<SelectListItem>();

        var animalRates = await _service.GetAnimalRatesAsync(isDefra);
        viewModel.AnimalTypeOptions = animalRates.Success && animalRates.Data != null
            ? animalRates.Data.Select(a => new SelectListItem(a.AnimalType, a.AnimalType)).ToList()
            : new List<SelectListItem>();

        var accountCats = await _service.GetAccountCategoriesAsync();
        viewModel.AccountCatOptions = accountCats.Success && accountCats.Data != null
            ? accountCats.Data.Select(c => new SelectListItem(c.AccShortName, c.AccShortName)).ToList()
            : new List<SelectListItem>();
    }

    private async Task<List<SelectListItem>> GetPayRateOptionsAsync(bool isDefra)
    {
        var response = await _service.GetPayRatesAsync(isDefra);
        return response.Success && response.Data != null
            ? response.Data.Select(p => new SelectListItem(p.WgGrade, p.WgGrade)).ToList()
            : new List<SelectListItem>();
    }
}
