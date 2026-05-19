using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupTestCapabilityControllerTest
{
    public class WorkGroupTestCapabilityControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestCapabilityService _service;
        private readonly WorkGroupTestCapabilityController _controller;

        public WorkGroupTestCapabilityControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<ITestCapabilityService>();
            _controller = new WorkGroupTestCapabilityController(
                _mapper,
                _service,
                Substitute.For<Apha.Common.Utilities.ExcelExport.IExcelExportService>());
        }

        private void SetupWorkGroupsResponse(List<WorkGroupDto> workGroups)
        {
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups));
        }

        private void SetupPagedTestCapabilityResponse(List<TestCapabilityDto> testCapabilities, PaginationDto? pagination = null)
        {
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                    testCapabilities,
                    pagination ?? new PaginationDto()));
        }

        private void SetupMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupTestCapabilityItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns(new List<WorkGroupTestCapabilityItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index

        [Fact]
        public async Task Index_WithWorkGroups_ReturnsViewWithWorkGroupOptions()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG001" },
                new() { WorkGroupName = "WG002" },
                new() { WorkGroupName = "WG003" }
            };
            SetupWorkGroupsResponse(workGroups);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Equal(3, model.WorkGroupOptions.Count);
            Assert.NotNull(model.TestCapabilityGrid);
        }

        [Fact]
        public async Task Index_WithNoWorkGroups_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            SetupWorkGroupsResponse(new List<WorkGroupDto>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WithFailedWorkGroupsResponse_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Service error" } },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow }));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WithNullWorkGroupsData_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(null!));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_Always_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            SetupWorkGroupsResponse(new List<WorkGroupDto>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.TestCapabilityGrid);
            Assert.Equal("testCapabilitiesWGGrid", model.TestCapabilityGrid.GridId);
            Assert.Empty(model.TestCapabilityGrid.Data);
        }

        [Fact]
        public async Task Index_WithWorkGroups_SetsCorrectSelectListItemText()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "TestWorkGroup" }
            };
            SetupWorkGroupsResponse(workGroups);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            var firstItem = model.WorkGroupOptions.First();
            Assert.Equal("TestWorkGroup", firstItem.Text);
            Assert.Equal("TestWorkGroup", firstItem.Value);
        }

        #endregion

        #region LoadTestCapabilityGrid

        [Fact]
        public async Task LoadTestCapabilityGrid_WithValidRequest_ReturnsPartialViewWithGrid()
        {
            // Arrange
            const string workGroup = "WG001";
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };
            var testCapabilities = new List<TestCapabilityDto>
            {
                new() { TestCode = "TC001", PlanPortfolio = "Portfolio1" },
                new() { TestCode = "TC002", PlanPortfolio = "Portfolio2" }
            };
            SetupPagedTestCapabilityResponse(testCapabilities);
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithNullWorkGroup_ReturnsPartialViewWithAllData()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Filter", "Invalid filter");

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);
            var successProperty = value!.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty.GetValue(value)!);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithEmptyResult_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithPaginationData_ReturnsGridWithPagination()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2,
                PageSize = 10,
                Filter = "{}",
                SortBy = "TestCode",
                Descending = true
            };
            var pagination = new PaginationDto
            {
                TotalRecords = 50,
                PageNumber = 2,
                PageSize = 10,
                TotalPages = 5
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>(), pagination);
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithFilteredData_CallsServiceWithCorrectParameters()
        {
            // Arrange
            const string workGroup = "WG001";
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"TestCode\":\"TC001\"}"
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            await _service.Received(1).GetPagedByWorkGroupAsync(
                Arg.Any<QueryParameters<string>>(),
                workGroup);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithNullFilter_UsesEmptyDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = null
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithEmptyFilterString_UsesEmptyDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithNullPagination_CreatesDefaultPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(new List<TestCapabilityDto>(), null));
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithFailedServiceResponse_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Service error" } },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow }));
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithSpecialCharactersInWorkGroup_HandlesCorrectly()
        {
            // Arrange
            const string workGroup = "WG-001/Test&Group";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
            await _service.Received(1).GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), workGroup);
        }

        #endregion

        #region Grid Configuration Tests

        [Fact]
        public async Task LoadTestCapabilityGrid_ConfiguresGridWithCorrectProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Equal("testCapabilitiesWGGrid", model.GridId);
            Assert.Equal("TestCode", model.KeyProperty);
            Assert.True(model.AllowRowSelection);
            Assert.Equal("onTestCapabilityRowSelect", model.RowSelectFunction);
            Assert.Equal("/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid", model.BindGridUrl);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_ConfiguresGridWithFilterMethod()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Equal("getTestCapabilityExtraFilters", model.ExtraFilterMethod);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_DisablesExportEditDeleteFlags()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.False(model.AllowExport);
            Assert.False(model.AllowEdit);
            Assert.False(model.AllowDelete);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_DoesNotSetCRUDFunctions()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.AddFunction));
            Assert.True(string.IsNullOrEmpty(model.EditFunction));
            Assert.True(string.IsNullOrEmpty(model.DeleteFunction));
            Assert.True(string.IsNullOrEmpty(model.ExportUrl));
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_EnablesPaginationAndRowSelection()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.True(model.ShowPagination);
            Assert.True(model.AllowRowSelection);
            Assert.False(model.ShowCheckboxColumn);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_SetsSortColumnsFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}",
                SortBy = "TestCode",
                Descending = true
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Equal("TestCode", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        #endregion

        #region Service Integration Tests

        [Fact]
        public async Task LoadTestCapabilityGrid_CallsGetPagedByWorkGroupAsync()
        {
            // Arrange
            const string workGroup = "WG001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            await _service.Received(1).GetPagedByWorkGroupAsync(
                Arg.Any<QueryParameters<string>>(),
                workGroup);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_MapsRequestToQueryParameters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(request);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_MapsResponseDataToItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var testCapabilities = new List<TestCapabilityDto>
            {
                new() { TestCode = "TC001" }
            };
            SetupPagedTestCapabilityResponse(testCapabilities);
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            _mapper.Received(1).Map<List<WorkGroupTestCapabilityItem>>(testCapabilities);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_MapsPaginationDto()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagination = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 5,
                TotalRecords = 50
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>(), pagination);
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            _mapper.Received(1).Map<PaginationModel>(pagination);
        }

        #endregion
    }
}
