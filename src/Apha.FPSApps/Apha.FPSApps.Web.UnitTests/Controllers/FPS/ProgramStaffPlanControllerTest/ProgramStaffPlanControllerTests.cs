using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProgramStaffPlanControllerTest
{
    public class ProgramStaffPlanControllerTests
    {
        private readonly IProgramService _programService;
        private readonly IAppStateService _appStateService;
        private readonly ProgramStaffPlanController _controller;

        public ProgramStaffPlanControllerTests()
        {
            _programService = Substitute.For<IProgramService>();
            _appStateService = Substitute.For<IAppStateService>();
            _controller = new ProgramStaffPlanController(_programService, _appStateService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static List<ProgramDto> BuildProgramList() =>
        [
            new() { ProgramNo = "P001", ProgramName = "Programme Alpha", Manager = "Alice", Target = 1000m },
            new() { ProgramNo = "P002", ProgramName = "Programme Beta",  Manager = "Bob",   Target = 2000m }
        ];

        #region Index Tests

        [Fact]
        public async Task Index_WithValidProgramNo_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var programNo = "P001";
            var programs = BuildProgramList();
            var programInfo = programs[0];

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(programInfo));

            // Act
            var result = await _controller.Index(programNo);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal("P001",            model.SelectedProgramNo);
            Assert.Equal("Programme Alpha", model.SelectedProgramme);
            Assert.Equal("Alice",           model.Manager);
            Assert.Equal(1000m,             model.Target);
            Assert.Equal(2,                 model.ProgrammeList.Count);
            Assert.NotNull(model.ProjectsGrid);
            Assert.NotNull(model.StaffCostGrid);
        }

        [Fact]
        public async Task Index_WithoutProgramNo_UsesEmptySelectedProgramNo()
        {
            // Arrange
            var programs = BuildProgramList();

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal(string.Empty, model.SelectedProgramNo);
            Assert.Equal(string.Empty, model.SelectedProgramme);
            Assert.Equal(string.Empty, model.Manager);
            Assert.Equal(2,            model.ProgrammeList.Count);
            await _programService.DidNotReceive().GetProgramByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WhenProgramListIsEmpty_UsesEmptySelectedProgramNo()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(
                    Enumerable.Empty<ProgramDto>()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal(string.Empty, model.SelectedProgramNo);
            Assert.Empty(model.ProgrammeList);
            Assert.Equal(string.Empty, model.SelectedProgramme);
        }

        [Fact]
        public async Task Index_WhenGetAllProgramsFails_UsesFallbackEmptyList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Empty(model.ProgrammeList);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_ProgrammeListItems_HaveCorrectValueAndTextFormat()
        {
            // Arrange
            var programs = BuildProgramList();

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);
            var firstItem = model.ProgrammeList[0];

            Assert.Equal("P001",                   firstItem.Value);
            Assert.Equal("P001 - Programme Alpha", firstItem.Text);
        }

        [Fact]
        public async Task Index_GridConfigs_HaveCorrectBindUrlsAndKeyProperties()
        {
            // Arrange
            var programs = BuildProgramList();

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal("/FPS/ProgramProject/LoadProjectGrid",                   model.ProjectsGrid.BindGridUrl);
            Assert.Equal("ParentProject",                                         model.ProjectsGrid.KeyProperty);
            Assert.Equal("/FPS/StaffJob/LoadStaffJobGrid?title=Staff%20Plan",     model.StaffCostGrid.BindGridUrl);
            Assert.Equal("StaffID",                                               model.StaffCostGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_GridConfigs_HaveCorrectGridIds()
        {
            // Arrange
            var programs = BuildProgramList();

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal("projectGrid",     model.ProjectsGrid.GridId);
            Assert.Equal("staffBookedGrid", model.StaffCostGrid.GridId);
        }

        [Fact]
        public async Task Index_StaffCostGrid_HasCorrectTitle()
        {
            // Arrange
            var programs = BuildProgramList();

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal("Staff Plan", model.StaffCostGrid.Title);
        }

        [Fact]
        public async Task Index_WithValidProgramNo_ProgramInfoIsLoadedFromService()
        {
            // Arrange
            var programNo = "P001";
            var programs = BuildProgramList();
            var programInfo = programs[0];

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(programInfo));

            // Act
            await _controller.Index(programNo);

            // Assert
            await _programService.Received(1).GetProgramByIdAsync(programNo);
        }

        [Fact]
        public async Task Index_WhenProgramInfoNotFound_UsesEmptyDefaults()
        {
            // Arrange
            var programs = BuildProgramList();

            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
            _programService.GetProgramByIdAsync("P999")
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(null));

            // Act
            var result = await _controller.Index("P999");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramStaffPlanViewModel>(viewResult.Model);

            Assert.Equal("P999",       model.SelectedProgramNo);
            Assert.Equal(string.Empty, model.SelectedProgramme);
            Assert.Equal(string.Empty, model.Manager);
            Assert.Equal(0m,           model.Target);
        }

        #endregion

        #region GetProgramInfo Tests

        [Fact]
        public async Task GetProgramInfo_WithValidProgramNo_ReturnsSuccessJson()
        {
            // Arrange
            var programNo = "P001";
            var programInfo = new ProgramDto
            {
                ProgramNo   = "P001",
                ProgramName = "Programme Alpha",
                Manager     = "Alice",
                Target      = 5000m
            };
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(programInfo));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme Alpha", value.GetProperty("programmeName").GetString());
            Assert.Equal("Alice",           value.GetProperty("manager").GetString());
            Assert.Equal(5000m,             value.GetProperty("target").GetDecimal());
            await _programService.Received(1).GetProgramByIdAsync(programNo);
        }

        [Fact]
        public async Task GetProgramInfo_WhenProgramNotFound_ReturnsFailureJson()
        {
            // Arrange
            var programNo = "P999";
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(null));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme not found.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetProgramInfo_WhenServiceFails_ReturnsNotFoundJson()
        {
            // Arrange
            var programNo = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme not found.", value.GetProperty("message").GetString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProgramInfo_WithEmptyOrWhitespaceProgramNo_ReturnsRequiredFailureJson(string programNo)
        {
            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme number is required.", value.GetProperty("message").GetString());
            await _programService.DidNotReceive().GetProgramByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProgramInfo_WithNullTarget_ReturnsZeroTarget()
        {
            // Arrange
            var programNo = "P001";
            var programInfo = new ProgramDto { ProgramNo = "P001", ProgramName = "Alpha", Manager = "Alice", Target = null };
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(programInfo));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(0m, value.GetProperty("target").GetDecimal());
        }

        #endregion
    }
}
