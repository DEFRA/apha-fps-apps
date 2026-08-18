using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/queries")]
    public class QueryReportController : ControllerBase
    {
        private readonly IQueryReportService _service;
        private readonly IMapper _mapper;

        public QueryReportController(IQueryReportService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all query reports (Type = 'Q') ordered by ReportDescription and SortOrder.
        /// </summary>
        /// <returns>List of QueryReportRes containing report metadata and filter options.</returns>
        [HttpGet("reports")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<QueryReportRes>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQueryReports()
        {
            var reports = await _service.GetQueryReportsAsync();
            var dto = _mapper.Map<List<Apha.PIMS.Application.Dtos.QueryReportDto>>(reports);
            var response = _mapper.Map<List<QueryReportRes>>(dto);
            return Ok(response);
        }

        /// <summary>
        /// Get monitoring report data using Access-style report year/month conversion.
        /// </summary>
        /// <param name="query">Pagination, sorting and JSON filter payload.</param>
        /// <param name="reportYear">Calendar year selected for the report period.</param>
        /// <param name="reportMonth">Calendar month (1-12) selected for the report period.</param>
        /// <param name="contractFilter">Contract wildcard filter (Access style, * = all).</param>
        /// <param name="programFilter">Optional list of program codes.</param>
        [HttpGet("monitoring")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationRes<MonitoringReportDataRes>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonitoringReportData(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] short reportYear,
            [FromQuery] short reportMonth,
            [FromQuery] string contractFilter = "*",
            [FromQuery] List<string>? programFilter = null)
        {
            if (reportMonth < 1 || reportMonth > 12)
            {
                return BadRequest("reportMonth must be between 1 and 12.");
            }

            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            short convertedReportYear = ConvertCalendarInputToAccessReportYear(reportYear, reportMonth);
            double convertedFiscalMonth = ConvertCalendarInputToAccessFiscalMonth(reportMonth);

            var pagedData = await _service.GetMonitoringReportDataAsync(
                parameters,
                convertedReportYear,
                convertedFiscalMonth,
                string.IsNullOrWhiteSpace(contractFilter) ? "*" : contractFilter,
                programFilter);

            var response = new PaginationRes<MonitoringReportDataRes>
            {
                Data = _mapper.Map<IEnumerable<MonitoringReportDataRes>>(pagedData.Data),
                PaginationData = _mapper.Map<Pagination>(pagedData.PaginationData),
                Total = pagedData.PaginationData.TotalRecords
            };

            return Ok(response);
        }

        /// <summary>
        /// Get Program and Customer Monitoring report data using Access-style report year/month conversion.
        /// Contract selection is intentionally ignored for this query type.
        /// </summary>
        [HttpGet("program-customer-monitoring")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationRes<ProgramCustomerMonitoringReportDataRes>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProgramCustomerMonitoringReportData(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] short reportYear,
            [FromQuery] short reportMonth,
            [FromQuery] List<string>? programFilter = null)
        {
            if (reportMonth < 1 || reportMonth > 12)
            {
                return BadRequest("reportMonth must be between 1 and 12.");
            }

            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            short convertedReportYear = ConvertCalendarInputToAccessReportYear(reportYear, reportMonth);
            double convertedFiscalMonth = ConvertCalendarInputToAccessFiscalMonth(reportMonth);

            var pagedData = await _service.GetProgramCustomerMonitoringReportDataAsync(
                parameters,
                convertedReportYear,
                convertedFiscalMonth,
                programFilter);

            var response = new PaginationRes<ProgramCustomerMonitoringReportDataRes>
            {
                Data = _mapper.Map<IEnumerable<ProgramCustomerMonitoringReportDataRes>>(pagedData.Data),
                PaginationData = _mapper.Map<Pagination>(pagedData.PaginationData),
                Total = pagedData.PaginationData.TotalRecords
            };

            return Ok(response);
        }

        private static short ConvertCalendarInputToAccessReportYear(short reportYear, short reportMonth)
            => (short)(reportMonth <= 3 ? reportYear - 1 : reportYear);

        private static double ConvertCalendarInputToAccessFiscalMonth(short reportMonth)
            => reportMonth <= 3 ? reportMonth + 9 : reportMonth - 3;
    }
}
