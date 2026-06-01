using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers;

[Area("CostBook")]
[Authorize(Roles = "CostbookAdmin,CostbookUser")]
[AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
public class ProjectSummaryController : Controller
{
    private readonly ICostBookYearlyDetailsService _yearlyDetailsService;
    private readonly ICostBookProjectSummaryService _projectSummaryService;

    public ProjectSummaryController(
        ICostBookYearlyDetailsService yearlyDetailsService,
        ICostBookProjectSummaryService projectSummaryService)
    {
        _yearlyDetailsService = yearlyDetailsService;
        _projectSummaryService = projectSummaryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string projectId)
    {
        var decodedProjectId = HttpUtility.UrlDecode(projectId);

        var headerResponse = await _yearlyDetailsService.GetProjectHeaderAsync(decodedProjectId);
        if (!headerResponse.Success || headerResponse.Data is null)
            return RedirectToAction("Index", "Projects");

        var yearsResponse = await _yearlyDetailsService.GetProjectYearsAsync(decodedProjectId);
        var yearDtos = yearsResponse.Success && yearsResponse.Data != null
            ? yearsResponse.Data.OrderBy(y => y.YearValue).ToList()
            : new List<Application.Dtos.CostBook.ProjectYearDto>();

        var allQuery = new QueryParameters<string> { Page = -1, PageSize = int.MaxValue };
        var rows = new List<ProjectSummaryRow>();

        foreach (var yearDto in yearDtos)
        {
            var year = yearDto.YearValue;

            var staffResponse      = await _yearlyDetailsService.GetStaffRequirementsAsync(decodedProjectId, year, allQuery);
            var testResponse       = await _yearlyDetailsService.GetTestRequirementsAsync(decodedProjectId, year,allQuery);
            var animalResponse     = await _yearlyDetailsService.GetAnimalRequirementsAsync(decodedProjectId, year, allQuery);
            var additionalResponse = await _yearlyDetailsService.GetAdditionalCostsAsync(decodedProjectId, year,allQuery);
            var profitResponse     = await _projectSummaryService.GetProfitIncludedTotalAsync(decodedProjectId, year);

            rows.Add(new ProjectSummaryRow
            {
                Year                = year,
                StaffCost           = staffResponse.Data?.data?.Sum(s => s.StaffCost ?? 0) ?? 0,
                TestCost            = testResponse.Data?.data?.Sum(t => t.TestCost ?? 0) ?? 0,
                AnimalCost          = animalResponse.Data?.data?.Sum(a => a.AnimalCost ?? 0) ?? 0,
                AdditionalCost      = additionalResponse.Data?.data?.Sum(ac => ac.CostEntered) ?? 0,
                ProfitIncludedTotal = profitResponse.Success ? profitResponse.Data : 0.0
            });
        }

        var viewModel = new ProjectSummaryViewModel
        {
            ProjectHeaderDto = headerResponse.Data,
            Rows             = rows,
            ShowInclProfit   = headerResponse.Data.Programme == "Comm"
        };

        return View(viewModel);
    }
}