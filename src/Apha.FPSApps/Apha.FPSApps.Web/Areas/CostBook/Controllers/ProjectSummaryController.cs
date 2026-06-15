using Apha.FPSApps.Application.Interfaces.Costbook;
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

        var rows = new List<ProjectSummaryRow>();

        foreach (var yearDto in yearDtos)
        {
            var year = yearDto.YearValue;

            var costResponse   = await _projectSummaryService.GetProjectYearCostSummaryAsync(decodedProjectId, year);
            var profitResponse = await _projectSummaryService.GetProfitIncludedTotalAsync(decodedProjectId, year);

            var cost = costResponse.Success && costResponse.Data is not null ? costResponse.Data : null;

            rows.Add(new ProjectSummaryRow
            {
                Year                = year,
                StaffCost           = cost?.StaffCostTotal      ?? 0,
                TestCost            = cost?.TestCostTotal        ?? 0,
                AnimalCost          = cost?.AnimalCostTotal      ?? 0,
                AdditionalCost      = cost?.AdditionalCostTotal  ?? 0,
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