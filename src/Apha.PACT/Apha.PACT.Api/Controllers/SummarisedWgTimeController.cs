using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/summarisedworkgrouptime")]
public class SummarisedWgTimeController : ControllerBase
{
    private readonly ISummarisedWgTimeService _service;
    private readonly IMapper _mapper;

    public SummarisedWgTimeController(ISummarisedWgTimeService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? workGroup)
    {
        SummarisedWgTimePivotDto result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
        return Ok(_mapper.Map<SummarisedWgTimePivotRes>(result));
    }
}
