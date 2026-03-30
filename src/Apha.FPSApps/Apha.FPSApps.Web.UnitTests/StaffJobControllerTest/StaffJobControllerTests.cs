using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.StaffJobControllerTest
{
    public class StaffJobControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;
        private readonly StaffJobController _controller;

        public StaffJobControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _staffJobService = Substitute.For<IStaffJobService>();
            _controller = new StaffJobController(_mapper, _staffJobService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region GetTotalStaffCost Tests

        [Fact]
        public async Task GetTotalStaffCost_WithValidJobCode_ReturnsSuccessJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var totalStaffCost = 4500m;
            var serviceResponse = ApiResponseDto<decimal>.SuccessResponse(totalStaffCost);

            _staffJobService.GetTotalStaffCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalStaffCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(totalStaffCost, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalStaffCost_WithEmptyJobCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalStaffCost(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required", value.GetProperty("message").GetString());
            Assert.Equal(0m, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.DidNotReceive().GetTotalStaffCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalStaffCost_WithWhitespaceJobCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalStaffCost("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required", value.GetProperty("message").GetString());
            await _staffJobService.DidNotReceive().GetTotalStaffCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalStaffCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API error", Code = "API_ERROR" }
            };
            var serviceResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());

            _staffJobService.GetTotalStaffCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalStaffCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve total staff cost.", value.GetProperty("message").GetString());
            Assert.Equal(0m, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Theory]
        [InlineData("JOB001", 1000.50)]
        [InlineData("FZ2000", 0.0)]
        [InlineData("PROJ123", 25000.75)]
        public async Task GetTotalStaffCost_WithVariousJobCodes_ReturnsCorrectTotal(string jobCode, double total)
        {
            // Arrange
            var expectedTotal = (decimal)total;
            var serviceResponse = ApiResponseDto<decimal>.SuccessResponse(expectedTotal);

            _staffJobService.GetTotalStaffCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalStaffCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(expectedTotal, value.GetProperty("totalStaffCost").GetDecimal());
            await _staffJobService.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        #endregion
    }
}
