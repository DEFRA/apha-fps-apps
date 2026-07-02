using System.Text.Json;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TotalBusinessOverheadsMaintenanceControllerTest
{
    public class TotalBusinessOverheadsMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITotalBusinessOverheadsService _service;
        private readonly TotalBusinessOverheadsMaintenanceController _controller;

        public TotalBusinessOverheadsMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<ITotalBusinessOverheadsService>();
            _controller = new TotalBusinessOverheadsMaintenanceController(_mapper, _service);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class JsonResultSuccess
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        private class JsonResultError
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public object? Errors { get; set; }
        }

        private static TotalBusinessOverheadsDto BuildDto(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        private static TotalBusinessOverheadsViewModel BuildViewModel(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TotalBusinessOverheadsMaintenanceController(null!, _service));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TotalBusinessOverheadsMaintenanceController(_mapper, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_WithSuccessfulGet_ReturnsViewWithViewModel()
        {
            // Arrange
            var dto = BuildDto();
            var viewModel = BuildViewModel();
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _service.GetAsync().Returns(apiResponse);
            _mapper.Map<TotalBusinessOverheadsViewModel>(dto).Returns(viewModel);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TotalBusinessOverheadsViewModel>(viewResult.Model);
            Assert.Equal(1000000m, model.TotalBusinessOverheads);
            Assert.Equal(2025, model.FpsYear);
        }

        [Fact]
        public async Task Index_WhenGetFails_ReturnsViewWithEmptyViewModel()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = false,
                Data = null
            };

            _service.GetAsync().Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TotalBusinessOverheadsViewModel>(viewResult.Model);
            Assert.Null(model.TotalBusinessOverheads);
            Assert.Equal(0, model.FpsYear);
        }

        [Fact]
        public async Task Index_WhenDataIsNull_ReturnsViewWithEmptyViewModel()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = true,
                Data = null
            };

            _service.GetAsync().Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TotalBusinessOverheadsViewModel>(viewResult.Model);
            Assert.Null(model.TotalBusinessOverheads);
        }

        [Fact]
        public async Task Index_CallsServiceGetAsync()
        {
            // Arrange
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(BuildDto());
            _service.GetAsync().Returns(apiResponse);
            _mapper.Map<TotalBusinessOverheadsViewModel>(Arg.Any<TotalBusinessOverheadsDto>())
                .Returns(BuildViewModel());

            // Act
            await _controller.Index();

            // Assert
            await _service.Received(1).GetAsync();
        }

        #endregion

        #region Save Tests

        [Fact]
        public async Task Save_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var viewModel = BuildViewModel(1500000m);
            var dto = BuildDto(1500000m);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mapper.Map<TotalBusinessOverheadsDto>(viewModel).Returns(dto);
            _service.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Save(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
            Assert.Equal("Total Business Overheads saved successfully.", value.Message);
        }

        [Fact]
        public async Task Save_WhenUpdateFails_ReturnsFailureJson()
        {
            // Arrange
            var viewModel = BuildViewModel();
            var dto = BuildDto();
            var apiResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = false
            };

            _mapper.Map<TotalBusinessOverheadsDto>(viewModel).Returns(dto);
            _service.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Save(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Failed to save Total Business Overheads.", value.Message);
        }

        [Fact]
        public async Task Save_WithInvalidModelState_ReturnsValidationErrorJson()
        {
            // Arrange
            var viewModel = BuildViewModel();
            _controller.ModelState.AddModelError("TotalBusinessOverheads", "Required");

            // Act
            var result = await _controller.Save(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Validation failed.", value.Message);
            Assert.NotNull(value.Errors);
        }

        [Fact]
        public async Task Save_CallsMapperAndService()
        {
            // Arrange
            var viewModel = BuildViewModel();
            var dto = BuildDto();
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mapper.Map<TotalBusinessOverheadsDto>(viewModel).Returns(dto);
            _service.UpdateAsync(dto).Returns(apiResponse);

            // Act
            await _controller.Save(viewModel);

            // Assert
            _mapper.Received(1).Map<TotalBusinessOverheadsDto>(viewModel);
            await _service.Received(1).UpdateAsync(dto);
        }

        [Fact]
        public async Task Save_WithNullOverheads_StillProcesses()
        {
            // Arrange
            var viewModel = BuildViewModel(null);
            var dto = BuildDto(null);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mapper.Map<TotalBusinessOverheadsDto>(viewModel).Returns(dto);
            _service.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Save(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task Save_WithZeroOverheads_StillProcesses()
        {
            // Arrange
            var viewModel = BuildViewModel(0m);
            var dto = BuildDto(0m);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mapper.Map<TotalBusinessOverheadsDto>(viewModel).Returns(dto);
            _service.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Save(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task Save_WithLargeValue_StillProcesses()
        {
            // Arrange
            var viewModel = BuildViewModel(999999999.99m);
            var dto = BuildDto(999999999.99m);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mapper.Map<TotalBusinessOverheadsDto>(viewModel).Returns(dto);
            _service.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Save(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        #endregion
    }
}
