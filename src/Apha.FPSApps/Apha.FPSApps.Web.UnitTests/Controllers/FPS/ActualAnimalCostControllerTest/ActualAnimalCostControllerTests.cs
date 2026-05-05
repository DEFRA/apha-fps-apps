using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ActualAnimalCostControllerTest
{
    public class ActualAnimalCostControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectSubContractService _projectSubContractService;
        private readonly IProjectService _projectService;
        private readonly ActualAnimalCostController _controller;

        public ActualAnimalCostControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectSubContractService = Substitute.For<IProjectSubContractService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ActualAnimalCostController(_mapper, _projectSubContractService, _projectService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region LoadActualAnimalCostGrid Tests

        [Fact]
        public async Task LoadActualAnimalCostGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var projectCode = "PROJ001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var subContracts = new List<ProjectSubContractDto>
            {
                new() { SubContCounter = 1, Project = projectCode, AcctCode = "LargeAnimals", Amount = 500m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(subContracts, paginationDto);
            var items = new List<ActualProjectCostItem> { new() { AcctCode = "LargeAnimals", Amount = 500m } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, projectCode).Returns(serviceResponse);
            _mapper.Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, projectCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            Assert.Equal("actualAnimalCostGrid", gridConfig.GridId);
            Assert.Equal("Actual Animal Costs (PACT)", gridConfig.Title);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, "PROJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _projectSubContractService.DidNotReceive()
                .GetFpsProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_WhenServiceReturnsNullData_MapsEmptyList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, "PROJ001").Returns(serviceResponse);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, "PROJ001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>());
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_WithNullProjectCode_PassesNullToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>(), new PaginationDto());
            var paginationModel = new PaginationModel();

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, null).Returns(serviceResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            await _projectSubContractService.Received(1).GetFpsProjectSubContractsAsync(queryParameters, null);
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_WhenPaginationMapReturnsNull_FallsBackToNewPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>(), paginationDto);

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, "PROJ001").Returns(serviceResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns((PaginationModel?)null);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, "PROJ001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            Assert.NotNull(gridConfig.Pagination);
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_SetsSortColumnAndDirectionOnPagination()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "Amount", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>(), new PaginationDto());
            var paginationModel = new PaginationModel();

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, "PROJ001").Returns(serviceResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, "PROJ001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            Assert.Equal("Amount", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_GridConfigHasDeleteOnlySettings()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>(), new PaginationDto());
            var paginationModel = new PaginationModel();

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, "PROJ001").Returns(serviceResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, "PROJ001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            Assert.False(gridConfig.AllowAdd);
            Assert.False(gridConfig.AllowEdit);
            Assert.True(gridConfig.AllowDelete);
            Assert.Equal("SubContCounter", gridConfig.KeyProperty);
            Assert.Equal("deleteActualAnimalCost", gridConfig.DeleteFunction);
            Assert.Equal("getActualAnimalExtraFilters", gridConfig.ExtraFilterMethod);
            Assert.Equal("/FPS/ActualAnimalCost/LoadActualAnimalCostGrid", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadActualAnimalCostGrid_WithFilterJson_ParsesFilterDictionary()
        {
            // Arrange
            var filterJson = "{\"AcctCode\":\"LargeAnimals\"}";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = filterJson };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>(), new PaginationDto());
            var paginationModel = new PaginationModel();

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, "PROJ001").Returns(serviceResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadActualAnimalCostGrid(request, "PROJ001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialView.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
            Assert.True(gridConfig.CurrentFilters.ContainsKey("AcctCode"));
            Assert.Equal("LargeAnimals", gridConfig.CurrentFilters["AcctCode"]);
        }

        #endregion

        #region GetProjectInfo Tests

        [Fact]
        public async Task GetProjectInfo_WithValidProjectCode_ReturnsProjectDetails()
        {
            // Arrange
            var projectCode = "PROJ001";
            var projectDto = new ProjectDto { ParentProject = projectCode, ProjectTitle = "Test Project", Program = "Prog1", Contract = "Cont1" };
            var serviceResponse = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _projectService.GetProjectByIdAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectInfo(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Test Project", value.GetProperty("projectTitle").GetString());
            Assert.Equal("Prog1", value.GetProperty("program").GetString());
            Assert.Equal("Cont1", value.GetProperty("contract").GetString());
        }

        [Fact]
        public async Task GetProjectInfo_WithEmptyProjectCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetProjectInfo(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _projectService.DidNotReceive().GetProjectByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectInfo_WhenServiceFails_ReturnsFailureJsonWithErrors()
        {
            // Arrange
            var projectCode = "PROJ001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProjectByIdAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectInfo(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Not found", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetProjectInfo_WhenServiceSucceedsButDataIsNull_ReturnsFailureJson()
        {
            // Arrange
            var projectCode = "PROJ001";
            var serviceResponse = new ApiResponseDto<ProjectDto> { Success = true, Data = null };

            _projectService.GetProjectByIdAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectInfo(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetProjectInfo_WithWhitespaceProjectCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetProjectInfo("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _projectService.DidNotReceive().GetProjectByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectInfo_WhenServiceFailsWithNullErrors_ReturnsFallbackMessage()
        {
            // Arrange
            var projectCode = "PROJ001";
            var serviceResponse = ApiResponseDto<ProjectDto>.FailureResponse(null, new ApiMetaDto());

            _projectService.GetProjectByIdAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectInfo(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Project not found.", value.GetProperty("message").GetString());
        }

        #endregion

        #region GetTotalActualCost Tests

        [Fact]
        public async Task GetTotalActualCost_WithWhitespaceProjectCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalActualCost("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("totalActualCost").GetInt32());
            await _projectSubContractService.DidNotReceive().GetFpsProjectSubContractTotalAmountAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalActualCost_WithValidProjectCode_ReturnsTotalCost()
        {
            // Arrange
            var projectCode = "PROJ001";
            var serviceResponse = ApiResponseDto<decimal>.SuccessResponse(1250.50m);
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalActualCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(1250.50m, value.GetProperty("totalActualCost").GetDecimal());
        }

        [Fact]
        public async Task GetTotalActualCost_WithEmptyProjectCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalActualCost(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("totalActualCost").GetInt32());
            await _projectSubContractService.DidNotReceive().GetFpsProjectSubContractTotalAmountAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalActualCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var projectCode = "PROJ001";
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            var serviceResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalActualCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Service error", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetTotalActualCost_WhenServiceFailsWithNoErrors_ReturnsFallbackMessage()
        {
            // Arrange
            var projectCode = "PROJ001";
            var serviceResponse = ApiResponseDto<decimal>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalActualCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Could not retrieve actual cost.", value.GetProperty("message").GetString());
            Assert.Equal(0, value.GetProperty("totalActualCost").GetInt32());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidSubContCounter_ReturnsSuccessJson()
        {
            // Arrange
            var subContCounter = 42;
            _projectSubContractService.DeleteAsync(subContCounter)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.Delete(subContCounter);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal sub-contract deleted successfully", value.GetProperty("message").GetString());
            await _projectSubContractService.Received(1).DeleteAsync(subContCounter);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var subContCounter = 99;
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            _projectSubContractService.DeleteAsync(subContCounter)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Delete(subContCounter);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Not found", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_WhenServiceFailsWithNoErrors_ReturnsFallbackMessage()
        {
            // Arrange
            var subContCounter = 5;
            _projectSubContractService.DeleteAsync(subContCounter)
                .Returns(ApiResponseDto<bool>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            // Act
            var result = await _controller.Delete(subContCounter);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete animal sub-contract.", value.GetProperty("message").GetString());
        }

        #endregion
    }
}
