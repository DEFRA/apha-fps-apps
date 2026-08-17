using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.QueriesControllerTest
{
    public class QueriesControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IQueriesService _queriesService;
        private readonly IRadTrackInvoiceService _radTrackInvoiceService;
        private readonly IProjectDetailsService _projectDetailsService;
        private readonly QueriesController _controller;

        public QueriesControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _queriesService = Substitute.For<IQueriesService>();
            _radTrackInvoiceService = Substitute.For<IRadTrackInvoiceService>();
            _projectDetailsService = Substitute.For<IProjectDetailsService>();
            _controller = new QueriesController(_mapper, _queriesService, _radTrackInvoiceService, _projectDetailsService);
        }

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static ApiResponseDto<T> SuccessResponse<T>(T data, PaginationDto? pagination = null)
            => ApiResponseDto<T>.SuccessResponse(data, pagination);

        private void SetupIndexLookups(List<string>? contracts = null, List<YearDto>? years = null)
        {
            _radTrackInvoiceService.GetContractsAsync().Returns(
                SuccessResponse(contracts ?? ["LabTGen", "R&DGen", "LabTGen"]));

            _projectDetailsService.GetAllYearAsync().Returns(
                SuccessResponse(years ??
                [
                    new YearDto { Value = 2024 },
                    new YearDto { Value = 2023 },
                    new YearDto { Value = 2025 }
                ]));
        }

        private void SetupMapperForGridBuild()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{}" });

            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
        }

        [Fact]
        public async Task Index_ReturnsView_WithQueriesViewModel()
        {
            // Arrange
            SetupIndexLookups();

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueriesViewModel>(view.Model);
            Assert.NotNull(model.QueryResultsGrid);
            Assert.NotEmpty(model.ContractOptions);
            Assert.NotEmpty(model.YearOptions);
        }

        [Fact]
        public async Task Index_CallsLookupServices_OnceEach()
        {
            // Arrange
            SetupIndexLookups();

            // Act
            await _controller.Index();

            // Assert
            await _radTrackInvoiceService.Received(1).GetContractsAsync();
            await _projectDetailsService.Received(1).GetAllYearAsync();
        }

        [Fact]
        public async Task Index_PopulatesContractOptions_WithSelectContract_AndDistinctOrderedValues()
        {
            // Arrange
            SetupIndexLookups(contracts: ["B", "A", "a", ""]);

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<QueriesViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("Select Contract", model.ContractOptions[0].Text);
            Assert.Equal("", model.ContractOptions[0].Value);
            Assert.Equal("A", model.ContractOptions[1].Text);
            Assert.Equal("B", model.ContractOptions[2].Text);
            Assert.Equal(3, model.ContractOptions.Count);
        }

        [Fact]
        public async Task LoadQueryResultsGrid_InvalidModelState_ReturnsJsonErrorPayload()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadQueryResultsGrid(new PaginationFilter<string>());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var payload = GetJsonElement(jsonResult);
            Assert.False(payload.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", payload.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadQueryResultsGrid_ProgramMonitoring_ReturnsProgramGrid_AndCallsDedicatedService()
        {
            // Arrange
            SetupMapperForGridBuild();

            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Program",
                Descending = false,
                Filter = "{}"
            };

            var serviceData = new List<ProgramCustomerMonitoringReportDataDto>
            {
                new() { ParentProject = "PP001", Program = "TB", PlannedCosts = 12m }
            };

            _queriesService.GetProgramCustomerMonitoringReportDataAsync(
                    Arg.Any<QueryParameters<string>>(), 2025, 8, Arg.Any<IEnumerable<string>?>())
                .Returns(SuccessResponse(serviceData, new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 }));

            _mapper.Map<List<ProgramCustomerMonitoringResultItem>>(Arg.Any<List<ProgramCustomerMonitoringReportDataDto>>())
                .Returns(new List<ProgramCustomerMonitoringResultItem> { new() { ParentProject = "PP001", Program = "TB" } });

            // Act
            var result = await _controller.LoadQueryResultsGrid(request, "8", "2025", "LabTGen", "programMonitoring");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProgramCustomerMonitoringResultItem>>(partial.Model);
            await _queriesService.Received(1).GetProgramCustomerMonitoringReportDataAsync(
                Arg.Any<QueryParameters<string>>(), 2025, 8, Arg.Any<IEnumerable<string>?>());
            await _queriesService.DidNotReceiveWithAnyArgs().GetMonitoringReportDataAsync(default!, default, default, default!, default);
        }

        [Fact]
        public async Task LoadQueryResultsGrid_NonProgramMonitoring_BlankContract_UsesWildcard()
        {
            // Arrange
            SetupMapperForGridBuild();

            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Program",
                Descending = true,
                Filter = "{}"
            };

            var serviceData = new List<MonitoringReportDataDto>
            {
                new() { ParentProject = "PP010", Program = "AMR", Contract = "R&DGen" }
            };

            _queriesService.GetMonitoringReportDataAsync(
                    Arg.Any<QueryParameters<string>>(), 2024, 12, "*", Arg.Any<IEnumerable<string>?>())
                .Returns(SuccessResponse(serviceData, new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 }));

            _mapper.Map<List<QueryResultItem>>(Arg.Any<List<MonitoringReportDataDto>>())
                .Returns(new List<QueryResultItem> { new() { ParentProject = "PP010", Program = "AMR" } });

            // Act
            var result = await _controller.LoadQueryResultsGrid(request, "12", "2024", "", "allContract");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<QueryResultItem>>(partial.Model);
            await _queriesService.Received(1).GetMonitoringReportDataAsync(
                Arg.Any<QueryParameters<string>>(), 2024, 12, "*", Arg.Any<IEnumerable<string>?>());
        }

        [Fact]
        public async Task LoadQueryResultsGrid_NonProgramMonitoring_WithContract_PassesContractToService()
        {
            // Arrange
            SetupMapperForGridBuild();

            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };

            _queriesService.GetMonitoringReportDataAsync(
                    Arg.Any<QueryParameters<string>>(), 2023, 1, "LabTGen", Arg.Any<IEnumerable<string>?>())
                .Returns(SuccessResponse(new List<MonitoringReportDataDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 0 }));

            _mapper.Map<List<QueryResultItem>>(Arg.Any<List<MonitoringReportDataDto>>())
                .Returns(new List<QueryResultItem>());

            // Act
            await _controller.LoadQueryResultsGrid(request, "1", "2023", "LabTGen", "contractMonitoring");

            // Assert
            await _queriesService.Received(1).GetMonitoringReportDataAsync(
                Arg.Any<QueryParameters<string>>(), 2023, 1, "LabTGen", Arg.Any<IEnumerable<string>?>());
        }

        [Fact]
        public async Task LoadQueryResultsGrid_AllContract_WithContract_IgnoresContractAndUsesWildcard()
        {
            // Arrange
            SetupMapperForGridBuild();

            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };

            _queriesService.GetMonitoringReportDataAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<short>(),
                    Arg.Any<short>(),
                    Arg.Any<string>(),
                    Arg.Any<IEnumerable<string>?>())
                .Returns(SuccessResponse(new List<MonitoringReportDataDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 0 }));

            _mapper.Map<List<QueryResultItem>>(Arg.Any<List<MonitoringReportDataDto>>())
                .Returns(new List<QueryResultItem>());

            // Act
            await _controller.LoadQueryResultsGrid(request, "1", "2023", "LabTGen", "allContract");

            // Assert
            await _queriesService.Received(1).GetMonitoringReportDataAsync(
                Arg.Any<QueryParameters<string>>(), 2023, 1, "*", Arg.Any<IEnumerable<string>?>());
            await _queriesService.DidNotReceive().GetMonitoringReportDataAsync(
                Arg.Any<QueryParameters<string>>(), 2023, 1, "LabTGen", Arg.Any<IEnumerable<string>?>());
        }

        [Fact]
        public async Task LoadQueryResultsGrid_ProgramMonitoring_InvalidMonthYear_DoesNotCallService_ReturnsPartial()
        {
            // Arrange
            SetupMapperForGridBuild();
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadQueryResultsGrid(request, "13", "ABC", null, "programMonitoring");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProgramCustomerMonitoringResultItem>>(partial.Model);
            await _queriesService.DidNotReceiveWithAnyArgs().GetProgramCustomerMonitoringReportDataAsync(default!, default, default, default);
        }
    }
}
