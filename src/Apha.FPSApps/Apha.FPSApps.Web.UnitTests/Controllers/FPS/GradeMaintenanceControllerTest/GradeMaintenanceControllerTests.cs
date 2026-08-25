using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.GradeMaintenanceControllerTest
{
    public class GradeMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IGradeService _gradeService;
        private readonly GradeMaintenanceController _controller;

        public GradeMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _gradeService = Substitute.For<IGradeService>();
            _controller = new GradeMaintenanceController(_mapper, _gradeService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private void SetupGradeServiceDefault()
        {
            _gradeService.GetAllPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<GradeDto>>.SuccessResponse(new List<GradeDto>()));
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GradeMaintenanceController(null!, _gradeService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGradeServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GradeMaintenanceController(_mapper, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupGradeServiceDefault();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_GridHasNoDefaultSortColumn()
        {
            // Arrange
            SetupGradeServiceDefault();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<GradeMaintenanceViewModel>(viewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.GradeGrid.Pagination.SortColumn),
                "No sort column should be applied on initial page load.");
        }

        [Fact]
        public async Task Index_GridHasNoDefaultSortDirection()
        {
            // Arrange
            SetupGradeServiceDefault();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<GradeMaintenanceViewModel>(viewResult.Model);
            Assert.False(model.GradeGrid.Pagination.SortDirection,
                "Sort direction should default to ascending (false) on initial page load.");
        }

        [Fact]
        public async Task Index_GridHasCorrectGridId()
        {
            // Arrange
            SetupGradeServiceDefault();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<GradeMaintenanceViewModel>(viewResult.Model);
            Assert.Equal("gradeGrid", model.GradeGrid.GridId);
        }

        [Fact]
        public async Task Index_CallsGetAllPagedAsync_Once()
        {
            // Arrange
            SetupGradeServiceDefault();

            // Act
            await _controller.Index();

            // Assert
            await _gradeService.Received(1).GetAllPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithEmptyId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Grade code is required", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceSucceeds_ReturnsJsonSuccess()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _gradeService.DeleteAsync("A").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Grade deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFailsWithNoErrors_ReturnsFallbackMessage()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _gradeService.DeleteAsync("A").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Unable to delete the grade as it may be in use.", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsDbPostgresError_ReturnsUserFriendlyFKMessage()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "DB_POSTGRES_ERROR", Message = "A database error occurred." }
            };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _gradeService.DeleteAsync("A").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("This grade cannot be deleted because it is referenced by other records.", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsOtherError_PropagatesErrorMessage()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "SOME_OTHER_ERROR", Message = "Some specific error." }
            };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _gradeService.DeleteAsync("A").Returns(apiResponse);

            // Act
            var result = await _controller.Delete("A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Some specific error.", value.message);
        }

        [Fact]
        public async Task Delete_CallsDeleteAsync_Once()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _gradeService.DeleteAsync("A").Returns(apiResponse);

            // Act
            await _controller.Delete("A");

            // Assert
            await _gradeService.Received(1).DeleteAsync("A");
        }

        #endregion

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}
