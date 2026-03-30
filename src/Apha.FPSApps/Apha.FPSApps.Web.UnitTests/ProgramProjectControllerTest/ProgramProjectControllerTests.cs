using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.ProgramProjectControllerTest
{
    public class ProgramProjectControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ProgramProjectController _controller;

        public ProgramProjectControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProgramProjectController(_mapper, _projectService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region LoadProjectGrid Tests

        [Fact]
        public async Task LoadProjectGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var projectViewModels = new List<ProjectViewModel>
            {
                new() { JobCode = "PP001", JobDescription = "Alpha Project" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(projects).Returns(projectViewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.Equal("projectGrid", gridConfig.GridId);
            Assert.Equal("Projects",    gridConfig.Title);
            Assert.Equal("JobCode",     gridConfig.KeyProperty);
            Assert.Single(gridConfig.Data);
            Assert.False(gridConfig.AllowAdd);
            Assert.False(gridConfig.AllowEdit);
            Assert.False(gridConfig.AllowDelete);
            Assert.True(gridConfig.AllowRowSelection);
        }

        [Fact]
        public async Task LoadProjectGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadProjectGrid(request, "P001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProjectGrid_WithNullProgramNo_UsesEmptyString()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, string.Empty).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _projectService.Received(1).GetProjectsByProgramAsync(queryParameters, string.Empty);
        }

        [Fact]
        public async Task LoadProjectGrid_WhenServiceReturnsFailure_MapsEmptyProjectList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>());
        }

        [Fact]
        public async Task LoadProjectGrid_SetsPaginationSortFields_FromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2, PageSize = 5, SortBy = "parentproject", Descending = true
            };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "parentproject", Descending = true
            };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 0 }
            );
            var paginationModel = new PaginationModel { PageNumber = 2, PageSize = 5 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.Equal("parentproject", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProjectGrid_WithJsonFilter_PassesFilterDictToGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"JobCode\":\"PP001\",\"JobDescription\":\"Alpha\"}"
            };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
            Assert.Equal("PP001", gridConfig.CurrentFilters["JobCode"]);
            Assert.Equal("Alpha",  gridConfig.CurrentFilters["JobDescription"]);
        }

        #endregion
    }
}
