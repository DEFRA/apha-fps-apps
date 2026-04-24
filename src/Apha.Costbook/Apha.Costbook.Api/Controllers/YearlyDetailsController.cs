using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers;

/// <summary>Project Year Details — converted from MS Access frmProject1.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/yearlydetails")]
[Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
public class YearlyDetailsController : ControllerBase
{
    private readonly IYearlyDetailsService _service;
    private readonly IMapper _mapper;

    public YearlyDetailsController(IYearlyDetailsService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // ── Project header ────────────────────────────────────────────────────────

    [HttpGet("{projectId}/header")]
    public async Task<IActionResult> GetProjectHeader(string projectId)
    {
        var dto = await _service.GetProjectHeaderAsync(projectId);
        if (dto is null) return NotFound();
        return Ok(BuildOk(_mapper.Map<ProjectHeaderRes>(dto)));
    }

    // ── Project years ─────────────────────────────────────────────────────────

    [HttpGet("{projectId}/years")]
    public async Task<IActionResult> GetProjectYears(string projectId)
    {
        var dtos = await _service.GetProjectYearsAsync(projectId);
        return Ok(BuildOk(_mapper.Map<List<ProjectYearRes>>(dtos)));
    }

    [HttpPost("{projectId}/years")]
    public async Task<IActionResult> AddProjectYear(string projectId, [FromBody] AddProjectYearReq req)
    {
        var dto = await _service.AddProjectYearAsync(projectId, req.Year);
        return Ok(BuildOk(_mapper.Map<ProjectYearRes>(dto)));
    }

    [HttpPut("{projectId}/years/{year}")]
    public async Task<IActionResult> UpdateProjectYear(string projectId, int year, [FromBody] ProjectYearReq req)
    {
        var dto = _mapper.Map<ProjectYearDto>(req);
        dto.Project = projectId;
        dto.YearValue = year;
        var updated = await _service.UpdateProjectYearAsync(dto);
        return Ok(BuildOk(_mapper.Map<ProjectYearRes>(updated)));
    }

    // ── Staff requirements ────────────────────────────────────────────────────

    [HttpGet("{projectId}/years/{year}/staff")]
    public async Task<IActionResult> GetStaffRequirements(
        string projectId, int year, [FromQuery] PaginationReq<string> query)
    {
        QueryParameters<string> filter = _mapper.Map<QueryParameters<string>>(query);
        PaginatedResult<StaffRequirementDto> result = await _service.GetStaffRequirementsAsync(projectId, year, filter);
        return Ok(BuildOk(_mapper.Map<PaginationRes<StaffRequirementRes>>(result)));
    }

    [HttpPost("{projectId}/years/{year}/staff")]
    public async Task<IActionResult> AddStaffRequirement(string projectId, int year, [FromBody] StaffRequirementReq req)
    {
        var dto = _mapper.Map<StaffRequirementDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        var result = await _service.AddStaffRequirementAsync(dto);
        return Ok(BuildOk(_mapper.Map<StaffRequirementRes>(result)));
    }

    [HttpPut("{projectId}/years/{year}/staff/{srIdentity}")]
    public async Task<IActionResult> UpdateStaffRequirement(string projectId, int year, int srIdentity, [FromBody] StaffRequirementReq req)
    {
        var dto = _mapper.Map<StaffRequirementDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        dto.SrIdentity = srIdentity;
        var result = await _service.UpdateStaffRequirementAsync(dto);
        return Ok(BuildOk(_mapper.Map<StaffRequirementRes>(result)));
    }

    [HttpDelete("{projectId}/years/{year}/staff/{srIdentity}")]
    public async Task<IActionResult> DeleteStaffRequirement(string projectId, int year, int srIdentity)
    {
        var deleted = await _service.DeleteStaffRequirementAsync(srIdentity);
        return Ok(BuildOk(deleted));
    }

    // ── Test requirements ─────────────────────────────────────────────────────

    [HttpGet("{projectId}/years/{year}/tests")]
    public async Task<IActionResult> GetTestRequirements(string projectId, int year)
    {
        var dtos = await _service.GetTestRequirementsAsync(projectId, year);
        return Ok(BuildOk(_mapper.Map<List<TestRequirementRes>>(dtos)));
    }

    [HttpPost("{projectId}/years/{year}/tests")]
    public async Task<IActionResult> AddTestRequirement(string projectId, int year, [FromBody] TestRequirementReq req)
    {
        var dto = _mapper.Map<TestRequirementDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        var result = await _service.AddTestRequirementAsync(dto);
        return Ok(BuildOk(_mapper.Map<TestRequirementRes>(result)));
    }

    [HttpPut("{projectId}/years/{year}/tests/{testCode}")]
    public async Task<IActionResult> UpdateTestRequirement(string projectId, int year, string testCode, [FromBody] TestRequirementReq req)
    {
        var dto = _mapper.Map<TestRequirementDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        dto.TestCode = testCode;
        var result = await _service.UpdateTestRequirementAsync(dto);
        return Ok(BuildOk(_mapper.Map<TestRequirementRes>(result)));
    }

    [HttpDelete("{projectId}/years/{year}/tests/{testCode}")]
    public async Task<IActionResult> DeleteTestRequirement(string projectId, int year, string testCode)
    {
        var deleted = await _service.DeleteTestRequirementAsync(projectId, year, testCode);
        return Ok(BuildOk(deleted));
    }

    // ── Animal requirements ───────────────────────────────────────────────────

    [HttpGet("{projectId}/years/{year}/animals")]
    public async Task<IActionResult> GetAnimalRequirements(string projectId, int year)
    {
        var dtos = await _service.GetAnimalRequirementsAsync(projectId, year);
        return Ok(BuildOk(_mapper.Map<List<AnimalRequirementRes>>(dtos)));
    }

    [HttpPost("{projectId}/years/{year}/animals")]
    public async Task<IActionResult> AddAnimalRequirement(string projectId, int year, [FromBody] AnimalRequirementReq req)
    {
        var dto = _mapper.Map<AnimalRequirementDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        var result = await _service.AddAnimalRequirementAsync(dto);
        return Ok(BuildOk(_mapper.Map<AnimalRequirementRes>(result)));
    }

    [HttpPut("{projectId}/years/{year}/animals/{arIdentity}")]
    public async Task<IActionResult> UpdateAnimalRequirement(string projectId, int year, int arIdentity, [FromBody] AnimalRequirementReq req)
    {
        var dto = _mapper.Map<AnimalRequirementDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        dto.ArIdentity = arIdentity;
        var result = await _service.UpdateAnimalRequirementAsync(dto);
        return Ok(BuildOk(_mapper.Map<AnimalRequirementRes>(result)));
    }

    [HttpDelete("{projectId}/years/{year}/animals/{arIdentity}")]
    public async Task<IActionResult> DeleteAnimalRequirement(string projectId, int year, int arIdentity)
    {
        var deleted = await _service.DeleteAnimalRequirementAsync(arIdentity);
        return Ok(BuildOk(deleted));
    }

    // ── Additional costs ──────────────────────────────────────────────────────

    [HttpGet("{projectId}/years/{year}/additionalcosts")]
    public async Task<IActionResult> GetAdditionalCosts(string projectId, int year)
    {
        var dtos = await _service.GetAdditionalCostsAsync(projectId, year);
        return Ok(BuildOk(_mapper.Map<List<AdditionalCostRes>>(dtos)));
    }

    [HttpPost("{projectId}/years/{year}/additionalcosts")]
    public async Task<IActionResult> AddAdditionalCost(string projectId, int year, [FromBody] AdditionalCostReq req)
    {
        var dto = _mapper.Map<AdditionalCostDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        var result = await _service.AddAdditionalCostAsync(dto);
        return Ok(BuildOk(_mapper.Map<AdditionalCostRes>(result)));
    }

    [HttpPut("{projectId}/years/{year}/additionalcosts/{acIdentity}")]
    public async Task<IActionResult> UpdateAdditionalCost(string projectId, int year, int acIdentity, [FromBody] AdditionalCostReq req)
    {
        var dto = _mapper.Map<AdditionalCostDto>(req);
        dto.Project = projectId;
        dto.Year = year;
        dto.AcIdentity = acIdentity;
        var result = await _service.UpdateAdditionalCostAsync(dto);
        return Ok(BuildOk(_mapper.Map<AdditionalCostRes>(result)));
    }

    [HttpDelete("{projectId}/years/{year}/additionalcosts/{acIdentity}")]
    public async Task<IActionResult> DeleteAdditionalCost(string projectId, int year, int acIdentity)
    {
        var deleted = await _service.DeleteAdditionalCostAsync(acIdentity);
        return Ok(BuildOk(deleted));
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    [HttpGet("lookups/payrates")]
    public async Task<IActionResult> GetPayRates([FromQuery] bool isDefra = false)
    {
        var dtos = await _service.GetPayRatesAsync(isDefra);
        return Ok(BuildOk(_mapper.Map<List<PayRateRes>>(dtos)));
    }

    [HttpGet("lookups/animalrates")]
    public async Task<IActionResult> GetAnimalRates([FromQuery] bool isDefra = false)
    {
        var dtos = await _service.GetAnimalRatesAsync(isDefra);
        return Ok(BuildOk(_mapper.Map<List<AnimalRateRes>>(dtos)));
    }

    [HttpGet("lookups/accountcategories")]
    public async Task<IActionResult> GetAccountCategories()
    {
        var dtos = await _service.GetAccountCategoriesAsync();
        return Ok(BuildOk(_mapper.Map<List<AccountCategoryRes>>(dtos)));
    }

    private static ApiResponse<T> BuildOk<T>(T data) => new()
    {
        Success = true,
        Data = data,
        Errors = new List<ApiError>(),
        Meta = new ApiMeta()
    };
}
