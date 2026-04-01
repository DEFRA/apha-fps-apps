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
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.UnitTests.Controllers
{
    public class AnimalJobControllerTests
    {
        private readonly IMapper _mapperMock;
        private readonly IProgramAnimalPlanService _serviceMock;
        private readonly AnimalJobController _controller;

        public AnimalJobControllerTests()
        {
            _mapperMock = Substitute.For<IMapper>();
            _serviceMock = Substitute.For<IProgramAnimalPlanService>();
            _controller = new AnimalJobController(_mapperMock, _serviceMock);
        }

        private static JsonElement GetJsonValue(IActionResult result)
        {
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonDocument.Parse(json).RootElement.Clone();
        }

        #region LoadAnimalPlanGrid

        [Fact]
        public async Task LoadAnimalPlanGrid_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Filter", "Invalid filter");

            // Act
            var result = await _controller.LoadAnimalPlanGrid(request, "JOB001");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_HappyPath_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string>();
            var pagedData = new ApiResponseDto<List<AnimalCostViewDto>>
            {
                Success = true,
                Data = new List<AnimalCostViewDto>
                {
                    new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", AnimalCost = 50m }
                },
                Pagination = new PaginationDto()
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<object>()).Returns(queryParams);
            _serviceMock.GetAllAnimalCostAsync(queryParams, "JOB001").Returns(pagedData);
            _mapperMock.Map<List<AnimalPlanItem>>(Arg.Any<object>()).Returns(new List<AnimalPlanItem>());
            _mapperMock.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAnimalPlanGrid(request, "JOB001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var queryParams = new QueryParameters<string>();
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<object>()).Returns(queryParams);
            _serviceMock.GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadAnimalPlanGrid(request, "JOB001"));
        }

        #endregion

        #region Create (GET)

        [Fact]
        public async Task Create_GET_ReturnsPartialView_WithAnimalDropdown()
        {
            // Arrange
            var lookupResponse = new ApiResponseDto<List<AnimalDto>>
            {
                Success = true,
                Data = new List<AnimalDto> { new() { AnimalType = "CAT" } }
            };
            _serviceMock.GetAnimalLookupAsync().Returns(lookupResponse);

            // Act
            var result = await _controller.Create();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalPlan", partialResult.ViewName);
        }

        [Fact]
        public async Task Create_GET_EmptyLookup_StillReturnsPartialView()
        {
            // Arrange
            var lookupResponse = new ApiResponseDto<List<AnimalDto>> { Success = true, Data = null };
            _serviceMock.GetAnimalLookupAsync().Returns(lookupResponse);

            // Act
            var result = await _controller.Create();

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        #endregion

        #region Create (POST)

        [Fact]
        public async Task Create_POST_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            var item = new AnimalPlanItem();
            _controller.ModelState.AddModelError("AnimalType", "Animal type is required");

            // Act
            var result = await _controller.Create(item);

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_POST_HappyPath_ReturnsJsonSuccess()
        {
            // Arrange
            var item = new AnimalPlanItem { AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 2 };
            var dto = new AnimalRequestDto { AnimalType = "CAT" };
            var response = new ApiResponseDto<AnimalRequestDto> { Success = true, Data = dto };

            _mapperMock.Map<AnimalRequestDto>(Arg.Any<object>()).Returns(dto);
            _serviceMock.CreateAnimalCostAsync(dto).Returns(response);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var value = GetJsonValue(result);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal cost created successfully", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_POST_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            var item = new AnimalPlanItem { AnimalType = "CAT" };
            var dto = new AnimalRequestDto { AnimalType = "CAT" };
            var response = new ApiResponseDto<AnimalRequestDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }
            };

            _mapperMock.Map<AnimalRequestDto>(Arg.Any<object>()).Returns(dto);
            _serviceMock.CreateAnimalCostAsync(dto).Returns(response);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_POST_ServiceThrows_PropagatesException()
        {
            // Arrange
            var item = new AnimalPlanItem { AnimalType = "CAT" };
            _mapperMock.Map<AnimalRequestDto>(Arg.Any<object>()).Returns(new AnimalRequestDto());
            _serviceMock.CreateAnimalCostAsync(Arg.Any<AnimalRequestDto>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(item));
        }

        #endregion

        #region Edit (GET)

        [Fact]
        public async Task Edit_GET_Success_ReturnsPartialView()
        {
            // Arrange
            var viewDto = new AnimalCostViewDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT" };
            var response = new ApiResponseDto<AnimalCostViewDto?> { Success = true, Data = viewDto };
            var model = new AnimalPlanItem { AnimalType = "CAT" };
            var lookupResponse = new ApiResponseDto<List<AnimalDto>>
            {
                Success = true,
                Data = new List<AnimalDto> { new() { AnimalType = "CAT" } }
            };

            _serviceMock.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(response);
            _mapperMock.Map<AnimalPlanItem>(Arg.Any<object>()).Returns(model);
            _serviceMock.GetAnimalLookupAsync().Returns(lookupResponse);

            // Act
            var result = await _controller.Edit(1, "JOB001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalPlan", partialResult.ViewName);
        }

        [Fact]
        public async Task Edit_GET_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            var response = new ApiResponseDto<AnimalCostViewDto?>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Not found" } }
            };
            _serviceMock.GetAnimalCostViewByIdAsync(999, "JOB001").Returns(response);

            // Act
            var result = await _controller.Edit(999, "JOB001");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_GET_DataIsNull_ReturnsJsonFalse()
        {
            // Arrange
            var response = new ApiResponseDto<AnimalCostViewDto?> { Success = true, Data = null };
            _serviceMock.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(response);

            // Act
            var result = await _controller.Edit(1, "JOB001");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Edit (POST)

        [Fact]
        public async Task Edit_POST_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            var item = new AnimalPlanItem();
            _controller.ModelState.AddModelError("AnimalType", "Animal type is required");

            // Act
            var result = await _controller.Edit(1, item);

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_POST_HappyPath_ReturnsJsonSuccess()
        {
            // Arrange
            var item = new AnimalPlanItem { AnimalType = "DOG", NumberOfDays = 3, NumberOfAnimals = 4 };
            var dto = new AnimalRequestDto { AnimalType = "DOG", IndCounter = 5 };
            var response = new ApiResponseDto<AnimalRequestDto> { Success = true, Data = dto };

            _mapperMock.Map<AnimalRequestDto>(Arg.Any<object>()).Returns(dto);
            _serviceMock.UpdateAnimalCostAsync(dto).Returns(response);

            // Act
            var result = await _controller.Edit(5, item);

            // Assert
            var value = GetJsonValue(result);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal cost updated successfully", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_POST_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            var item = new AnimalPlanItem { AnimalType = "CAT" };
            var dto = new AnimalRequestDto { AnimalType = "CAT" };
            var response = new ApiResponseDto<AnimalRequestDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Update failed" } }
            };

            _mapperMock.Map<AnimalRequestDto>(Arg.Any<object>()).Returns(dto);
            _serviceMock.UpdateAnimalCostAsync(dto).Returns(response);

            // Act
            var result = await _controller.Edit(1, item);

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_Success_ReturnsJsonTrue()
        {
            // Arrange
            var response = new ApiResponseDto<bool> { Success = true, Data = true };
            _serviceMock.DeleteAnimalCostAsync(1).Returns(response);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var value = GetJsonValue(result);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal cost deleted successfully", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            var response = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Not found" } }
            };
            _serviceMock.DeleteAnimalCostAsync(999).Returns(response);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Delete_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteAnimalCostAsync(Arg.Any<int>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(1));
        }

        #endregion

        #region GetAnimalRate

        [Fact]
        public async Task GetAnimalRate_EmptyAnimalType_ReturnsJsonFalse()
        {
            // Act
            var result = await _controller.GetAnimalRate("");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal type is required", value.GetProperty("message").GetString());
            Assert.Equal(0, value.GetProperty("dailyRate").GetDecimal());
        }

        [Fact]
        public async Task GetAnimalRate_HappyPath_ReturnsJsonWithRate()
        {
            // Arrange
            var response = new ApiResponseDto<decimal?> { Success = true, Data = 75.50m };
            _serviceMock.GetAnimalRateAsync("CAT").Returns(response);

            // Act
            var result = await _controller.GetAnimalRate("CAT");

            // Assert
            var value = GetJsonValue(result);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(75.50m, value.GetProperty("dailyRate").GetDecimal());
        }

        [Fact]
        public async Task GetAnimalRate_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            var response = new ApiResponseDto<decimal?>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Rate not found" } }
            };
            _serviceMock.GetAnimalRateAsync("UNKNOWN").Returns(response);

            // Act
            var result = await _controller.GetAnimalRate("UNKNOWN");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("dailyRate").GetDecimal());
        }

        [Fact]
        public async Task GetAnimalRate_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAnimalRateAsync(Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalRate("CAT"));
        }

        #endregion

        #region GetTotalAnimalCost

        [Fact]
        public async Task GetTotalAnimalCost_EmptyJobCode_ReturnsJsonFalse()
        {
            // Act
            var result = await _controller.GetTotalAnimalCost("");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required", value.GetProperty("message").GetString());
            Assert.Equal(0, value.GetProperty("totalAnimalCost").GetDecimal());
        }

        [Fact]
        public async Task GetTotalAnimalCost_HappyPath_ReturnsJsonWithTotal()
        {
            // Arrange
            var response = new ApiResponseDto<decimal> { Success = true, Data = 250m };
            _serviceMock.GetTotalAnimalCostAsync("JOB001").Returns(response);

            // Act
            var result = await _controller.GetTotalAnimalCost("JOB001");

            // Assert
            var value = GetJsonValue(result);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(250m, value.GetProperty("totalAnimalCost").GetDecimal());
        }

        [Fact]
        public async Task GetTotalAnimalCost_ServiceFails_ReturnsJsonFalse()
        {
            // Arrange
            var response = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }
            };
            _serviceMock.GetTotalAnimalCostAsync("JOB001").Returns(response);

            // Act
            var result = await _controller.GetTotalAnimalCost("JOB001");

            // Assert
            var value = GetJsonValue(result);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("totalAnimalCost").GetDecimal());
        }

        [Fact]
        public async Task GetTotalAnimalCost_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetTotalAnimalCostAsync(Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTotalAnimalCost("JOB001"));
        }

        #endregion
    }
}
