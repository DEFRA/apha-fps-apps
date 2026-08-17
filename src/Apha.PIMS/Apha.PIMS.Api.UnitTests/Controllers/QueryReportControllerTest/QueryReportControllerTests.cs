using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PIMS.Api.UnitTests.Controllers.QueryReportControllerTest
{
    public class QueryReportControllerTests
    {
        private readonly IQueryReportService _service;
        private readonly IMapper _mapper;
        private readonly QueryReportController _controller;

        public QueryReportControllerTests()
        {
            _service = Substitute.For<IQueryReportService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new QueryReportController(_service, _mapper);
        }

        [Fact]
        public async Task GetQueryReports_ReturnsOk_WithMappedResponse()
        {
            var reports = new List<QueryReportItem>
            {
                new() { ReportName = "R1", ReportDescription = "Report 1" }
            };
            var dto = new List<QueryReportDto>
            {
                new() { ReportName = "R1", ReportDescription = "Report 1" }
            };
            var response = new List<QueryReportRes>
            {
                new() { ReportName = "R1", ReportDescription = "Report 1" }
            };

            _service.GetQueryReportsAsync().Returns(reports);
            _mapper.Map<List<QueryReportDto>>(reports).Returns(dto);
            _mapper.Map<List<QueryReportRes>>(dto).Returns(response);

            var result = await _controller.GetQueryReports();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
            await _service.Received(1).GetQueryReportsAsync();
            _mapper.Received(1).Map<List<QueryReportDto>>(reports);
            _mapper.Received(1).Map<List<QueryReportRes>>(dto);
        }

        [Fact]
        public async Task GetMonitoringReportData_ConvertsJanInput_UsesWildcardForBlankContract_AndReturnsPagedResult()
        {
            var query = new PaginationReq<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}",
                SortBy = "ParentProject",
                Descending = false
            };

            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}",
                SortBy = "ParentProject",
                Descending = false
            };

            var data = new List<MonitoringReportData>
            {
                new() { Program = "P1", ParentProject = "PP001" }
            };

            var paged = new PagedData<MonitoringReportData>(
                data,
                new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1,
                    TotalRecords = 1
                });

            var responseRows = new List<MonitoringReportDataRes>
            {
                new() { Program = "P1", ParentProject = "PP001" }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _service.GetMonitoringReportDataAsync(parameters, 2024, 10, "*", null).Returns(paged);
            _mapper.Map<IEnumerable<MonitoringReportDataRes>>(paged.Data).Returns(responseRows);
            _mapper.Map<Pagination>(paged.PaginationData).Returns(pagination);

            var result = await _controller.GetMonitoringReportData(query, 2025, 1, string.Empty, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<PaginationRes<MonitoringReportDataRes>>(okResult.Value);
            Assert.Equal(responseRows, payload.Data);
            Assert.Equal(pagination, payload.PaginationData);
            Assert.Equal(1, payload.Total);

            await _service.Received(1).GetMonitoringReportDataAsync(parameters, 2024, 10, "*", null);
        }

        [Fact]
        public async Task GetMonitoringReportData_UsesProvidedContract_AndConvertsAprilToFiscalMonthOne()
        {
            var query = new PaginationReq<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var programFilter = new List<string> { "TB" };

            var paged = new PagedData<MonitoringReportData>(
                new List<MonitoringReportData>(),
                new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0,
                    TotalRecords = 0
                });

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _service.GetMonitoringReportDataAsync(parameters, 2025, 1, "LabTGen", programFilter).Returns(paged);
            _mapper.Map<IEnumerable<MonitoringReportDataRes>>(paged.Data).Returns(new List<MonitoringReportDataRes>());
            _mapper.Map<Pagination>(paged.PaginationData).Returns(new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            var result = await _controller.GetMonitoringReportData(query, 2025, 4, "LabTGen", programFilter);

            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).GetMonitoringReportDataAsync(parameters, 2025, 1, "LabTGen", programFilter);
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)13)]
        public async Task GetMonitoringReportData_WithInvalidMonth_ReturnsBadRequest(short invalidMonth)
        {
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };

            var result = await _controller.GetMonitoringReportData(query, 2025, invalidMonth, "*", null);

            Assert.IsType<BadRequestObjectResult>(result);
            await _service.DidNotReceive().GetMonitoringReportDataAsync(
                Arg.Any<PaginationParameters<string>>(),
                Arg.Any<short>(),
                Arg.Any<double>(),
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>?>());
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportData_ConvertsJanInput_AndReturnsPagedResult()
        {
            var query = new PaginationReq<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}",
                SortBy = "ParentProject",
                Descending = false
            };

            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}",
                SortBy = "ParentProject",
                Descending = false
            };

            var data = new List<ProgramCustomerMonitoringReportData>
            {
                new() { Program = "P1", ParentProject = "PP001" }
            };

            var paged = new PagedData<ProgramCustomerMonitoringReportData>(
                data,
                new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1,
                    TotalRecords = 1
                });

            var responseRows = new List<ProgramCustomerMonitoringReportDataRes>
            {
                new() { Program = "P1", ParentProject = "PP001" }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _service.GetProgramCustomerMonitoringReportDataAsync(parameters, 2024, 10, null).Returns(paged);
            _mapper.Map<IEnumerable<ProgramCustomerMonitoringReportDataRes>>(paged.Data).Returns(responseRows);
            _mapper.Map<Pagination>(paged.PaginationData).Returns(pagination);

            var result = await _controller.GetProgramCustomerMonitoringReportData(query, 2025, 1, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<PaginationRes<ProgramCustomerMonitoringReportDataRes>>(okResult.Value);
            Assert.Equal(responseRows, payload.Data);
            Assert.Equal(pagination, payload.PaginationData);
            Assert.Equal(1, payload.Total);

            await _service.Received(1).GetProgramCustomerMonitoringReportDataAsync(parameters, 2024, 10, null);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportData_ConvertsAprilToFiscalMonthOne_AndPassesProgramFilter()
        {
            var query = new PaginationReq<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var programFilter = new List<string> { "AMR", "TB" };

            var paged = new PagedData<ProgramCustomerMonitoringReportData>(
                new List<ProgramCustomerMonitoringReportData>(),
                new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0,
                    TotalRecords = 0
                });

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _service.GetProgramCustomerMonitoringReportDataAsync(parameters, 2025, 1, programFilter).Returns(paged);
            _mapper.Map<IEnumerable<ProgramCustomerMonitoringReportDataRes>>(paged.Data).Returns(new List<ProgramCustomerMonitoringReportDataRes>());
            _mapper.Map<Pagination>(paged.PaginationData).Returns(new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            var result = await _controller.GetProgramCustomerMonitoringReportData(query, 2025, 4, programFilter);

            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).GetProgramCustomerMonitoringReportDataAsync(parameters, 2025, 1, programFilter);
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)13)]
        public async Task GetProgramCustomerMonitoringReportData_WithInvalidMonth_ReturnsBadRequest(short invalidMonth)
        {
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };

            var result = await _controller.GetProgramCustomerMonitoringReportData(query, 2025, invalidMonth, null);

            Assert.IsType<BadRequestObjectResult>(result);
            await _service.DidNotReceive().GetProgramCustomerMonitoringReportDataAsync(
                Arg.Any<PaginationParameters<string>>(),
                Arg.Any<short>(),
                Arg.Any<double>(),
                Arg.Any<IEnumerable<string>?>());
        }
    }
}
