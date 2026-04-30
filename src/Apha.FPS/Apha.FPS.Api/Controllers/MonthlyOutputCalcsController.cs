using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/monthlyoutputcalcs")]
    public class MonthlyOutputCalcsController : ControllerBase
    {
        private readonly IMonthlyOutputCalcsService _service;
        private readonly IMapper _mapper;

        public MonthlyOutputCalcsController(IMonthlyOutputCalcsService service, IMapper mapper)
        {
            _service = service;
            _mapper  = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetByProjectAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new ArgumentException("projectCode is required.");

            var result = await _service.GetByProjectAsync(query, projectCode);
            return Ok(_mapper.Map<PaginationRes<MonthlyOutputCalcsViewRes>>(result));
        }

        [HttpGet("totals")]
        public async Task<IActionResult> GetTotalActualByProjectAsync([FromQuery] string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new ArgumentException("projectCode is required.");

            var result = await _service.GetTotalActualByProjectAsync(projectCode);
            return Ok(_mapper.Map<MonthlyOutputCalcsTotalsRes>(result));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] MonthlyOutputCalcsReq req)
        {
            if (req == null)
                throw new ArgumentException("Request body is required.");

            if (string.IsNullOrWhiteSpace(req.Buyer) || string.IsNullOrWhiteSpace(req.TestCode)
                || string.IsNullOrWhiteSpace(req.WorkGroup))
                throw new ArgumentException("Buyer, TestCode and WorkGroup are required.");

            var deleted = await _service.DeleteAsync(req.Buyer, req.TestCode, req.Month, req.WorkGroup);
            if (!deleted)
                throw new KeyNotFoundException("Record not found.");

            return Ok(deleted);
        }
    }
}