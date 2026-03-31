using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.UnitTests.Controllers
{
    public class ProgramAnimalPlanControllerTests
    {
        private readonly IProgramService _programServiceMock;
        private readonly ProgramAnimalPlanController _controller;

        public ProgramAnimalPlanControllerTests()
        {
            _programServiceMock = Substitute.For<IProgramService>();
            _controller = new ProgramAnimalPlanController(_programServiceMock);
        }

        private static JsonElement GetJsonValue(IActionResult result)
        {
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonDocument.Parse(json).RootElement.Clone();
        }

        #region Index

        [Fact]
        public async Task Index_WithProgramNo_SetsCorrectViewModelProperties()
        {
            // Arrange
            var programDto = new ProgramDto
            {
                ProgramNo = "PROG01",
                ProgramName = "Test Programme",
                Manager = "John Smith",
                Target = 1000m
            };
            var programList = new ApiResponseDto<IEnumerable<ProgramDto>>
            {
                Success = true,
                Data = new List<ProgramDto> { programDto }
            };
            var programInfo = new ApiResponseDto<ProgramDto?> { Success = true, Data = programDto };

            _programServiceMock.GetAllProgramsAsync().Returns(programList);
            _programServiceMock.GetProgramByIdAsync("PROG01").Returns(programInfo);

            // Act
            var result = await _controller.Index("PROG01");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramAnimalPlanViewModel>(viewResult.Model);
            Assert.Equal("PROG01", model.SelectedProgramNo);
            Assert.Equal("Test Programme", model.SelectedProgramme);
            Assert.Equal("John Smith", model.Manager);
            Assert.Equal(1000m, model.Target);
        }

        [Fact]
        public async Task Index_NullProgramNo_UsesFirstProgrammeFromList()
        {
            // Arrange
            var programDto = new ProgramDto { ProgramNo = "FIRST01", ProgramName = "First Programme" };
            var programList = new ApiResponseDto<IEnumerable<ProgramDto>>
            {
                Success = true,
                Data = new List<ProgramDto> { programDto }
            };
            var programInfo = new ApiResponseDto<ProgramDto?> { Success = true, Data = programDto };

            _programServiceMock.GetAllProgramsAsync().Returns(programList);
            _programServiceMock.GetProgramByIdAsync("FIRST01").Returns(programInfo);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramAnimalPlanViewModel>(viewResult.Model);
            Assert.Equal("FIRST01", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_EmptyProgrammeList_UsesEmptyStringAndSetsDefaultModel()
        {
            // Arrange
            var programList = new ApiResponseDto<IEnumerable<ProgramDto>>
            {
                Success = true,
                Data = new List<ProgramDto>()
            };

            _programServiceMock.GetAllProgramsAsync().Returns(programList);
            _programServiceMock.GetProgramByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProgramDto?> { Success = false });

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramAnimalPlanViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
            Assert.Equal(string.Empty, model.SelectedProgramme);
            Assert.Equal(string.Empty, model.Manager);
        }

        [Fact]
        public async Task Index_ProgramInfoNotFound_SetsDefaultModelProperties()
        {
            // Arrange
            var programDto = new ProgramDto { ProgramNo = "PROG01" };
            var programList = new ApiResponseDto<IEnumerable<ProgramDto>>
            {
                Success = true,
                Data = new List<ProgramDto> { programDto }
            };
            var notFoundResponse = new ApiResponseDto<ProgramDto?> { Success = false, Data = null };

            _programServiceMock.GetAllProgramsAsync().Returns(programList);
            _programServiceMock.GetProgramByIdAsync("PROG01").Returns(notFoundResponse);

            // Act
            var result = await _controller.Index("PROG01");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramAnimalPlanViewModel>(viewResult.Model);
            Assert.Equal("PROG01", model.SelectedProgramNo);
            Assert.Equal(string.Empty, model.SelectedProgramme);
            Assert.Equal(string.Empty, model.Manager);
            Assert.Equal(0m, model.Target);
        }

        [Fact]
        public async Task Index_ProgrammeListFailure_UsesEmptyList()
        {
            // Arrange
            var failureResponse = new ApiResponseDto<IEnumerable<ProgramDto>> { Success = false };
            _programServiceMock.GetAllProgramsAsync().Returns(failureResponse);
            _programServiceMock.GetProgramByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProgramDto?> { Success = false });

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramAnimalPlanViewModel>(viewResult.Model);
            Assert.Empty(model.ProgrammeList);
        }

        [Fact]
        public async Task Index_BuildsBothDataGridConfigs()
        {
            // Arrange
            var programList = new ApiResponseDto<IEnumerable<ProgramDto>>
            {
                Success = true,
                Data = new List<ProgramDto> { new() { ProgramNo = "PROG01" } }
            };
            _programServiceMock.GetAllProgramsAsync().Returns(programList);
            _programServiceMock.GetProgramByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProgramDto?> { Success = false });

            // Act
            var result = await _controller.Index("PROG01");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramAnimalPlanViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectsGrid);
            Assert.NotNull(model.AnimalCostGrid);
            Assert.Equal("projectGrid", model.ProjectsGrid.GridId);
            Assert.Equal("animalBookedGrid", model.AnimalCostGrid.GridId);
        }

        #endregion

        #region GetProgramInfo

        [Fact]
        public async Task GetProgramInfo_EmptyProgramNo_ReturnsJsonFalse()
        {
            // Act
            var result = await _controller.GetProgramInfo("");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme number is required.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetProgramInfo_WhitespaceProgramNo_ReturnsJsonFalse()
        {
            // Act
            var result = await _controller.GetProgramInfo("   ");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetProgramInfo_ProgramFound_ReturnsJsonWithData()
        {
            // Arrange
            var programDto = new ProgramDto
            {
                ProgramNo = "PROG01",
                ProgramName = "Test Programme",
                Manager = "John Smith",
                Target = 2500m
            };
            _programServiceMock.GetProgramByIdAsync("PROG01")
                .Returns(new ApiResponseDto<ProgramDto?> { Success = true, Data = programDto });

            // Act
            var result = await _controller.GetProgramInfo("PROG01");

            // Assert
            var value = GetJsonValue(result);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Test Programme", value.GetProperty("programmeName").GetString());
            Assert.Equal("John Smith", value.GetProperty("manager").GetString());
            Assert.Equal(2500m, value.GetProperty("target").GetDecimal());
        }

        [Fact]
        public async Task GetProgramInfo_ProgramNotFound_ReturnsJsonFalse()
        {
            // Arrange
            _programServiceMock.GetProgramByIdAsync("UNKNOWN")
                .Returns(new ApiResponseDto<ProgramDto?> { Success = true, Data = null });

            // Act
            var result = await _controller.GetProgramInfo("UNKNOWN");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme not found.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetProgramInfo_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            _programServiceMock.GetProgramByIdAsync("PROG01")
                .Returns(new ApiResponseDto<ProgramDto?> { Success = false, Data = null });

            // Act
            var result = await _controller.GetProgramInfo("PROG01");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetProgramInfo_ServiceThrows_PropagatesException()
        {
            // Arrange
            _programServiceMock.GetProgramByIdAsync(Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProgramInfo("PROG01"));
        }

        #endregion
    }
}
