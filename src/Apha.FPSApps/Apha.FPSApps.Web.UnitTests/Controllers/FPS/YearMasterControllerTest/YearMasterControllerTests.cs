using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.YearMasterControllerTest
{
    public class YearMasterControllerTests
    {
        private readonly IYearMasterService _yearMasterService;
        private readonly YearMasterController _controller;

        public YearMasterControllerTests()
        {
            _yearMasterService = Substitute.For<IYearMasterService>();
            _controller = new YearMasterController(_yearMasterService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region GetYearMasterDropdown Tests

        [Fact]
        public async Task GetYearMasterDropdown_HappyPath_ReturnsSuccessJson()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            
            var data = value.GetProperty("data");
            Assert.Equal(3, data.GetArrayLength());

            // Verify descending order
            var firstItem = data[0];
            Assert.Equal(2025, firstItem.GetProperty("value").GetInt32());
            Assert.Equal("2025", firstItem.GetProperty("text").GetString());
            Assert.Equal("Planned", firstItem.GetProperty("yearStatus").GetString());

            var lastItem = data[2];
            Assert.Equal(2023, lastItem.GetProperty("value").GetInt32());

            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithActiveYearsOnly_ExcludesInactiveYears()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = false }, // Inactive
                new YearMasterDto { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = false }  // Inactive
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            
            var data = value.GetProperty("data");
            Assert.Equal(1, data.GetArrayLength()); // Only 1 active year

            var firstItem = data[0];
            Assert.Equal(2024, firstItem.GetProperty("value").GetInt32());
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithDescendingOrder_ReturnsNewestFirst()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2021, FpsYearCode = "2021", YearStatus = "Closed", Active = true },
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            var data = value.GetProperty("data");
            Assert.Equal(2025, data[0].GetProperty("value").GetInt32()); // Newest first
            Assert.Equal(2023, data[1].GetProperty("value").GetInt32());
            Assert.Equal(2021, data[2].GetProperty("value").GetInt32()); // Oldest last
        }

        [Fact]
        public async Task GetYearMasterDropdown_EdgeCase_EmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var emptyYearMasters = new List<YearMasterDto>();
            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(emptyYearMasters);

            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            var data = value.GetProperty("data");
            Assert.Equal(0, data.GetArrayLength());

            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_EdgeCase_AllInactive_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = false },
                new YearMasterDto { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = false }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            var data = value.GetProperty("data");
            Assert.Equal(0, data.GetArrayLength()); // All filtered out
        }

        [Fact]
        public async Task GetYearMasterDropdown_EdgeCase_NullData_ReturnsFailureJson()
        {
            // Arrange
            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse((IEnumerable<YearMasterDto>)null!);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve year masters", value.GetProperty("message").GetString());

            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API error", Code = "API_ERROR" }
            };
            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.FailureResponse(errors, new ApiMetaDto());

            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve year masters", value.GetProperty("message").GetString());

            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithSingleYear_ReturnsSingleItem()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            var data = value.GetProperty("data");
            Assert.Equal(1, data.GetArrayLength());
            Assert.Equal(2024, data[0].GetProperty("value").GetInt32());
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithMixedStatuses_ReturnsAllActiveYears()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            var data = value.GetProperty("data");
            Assert.Equal(3, data.GetArrayLength());

            // Verify all statuses are present
            var statuses = new List<string>();
            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                statuses.Add(data[i].GetProperty("yearStatus").GetString()!);
            }

            Assert.Contains("Planned", statuses);
            Assert.Contains("Open", statuses);
            Assert.Contains("Closed", statuses);
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithMixedActiveInactive_ReturnsOnlyActive()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = false }, // Inactive
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true },
                new YearMasterDto { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed", Active = false }  // Inactive
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            var data = value.GetProperty("data");
            Assert.Equal(2, data.GetArrayLength()); // Only 2 active years

            // Verify returned years are 2025 and 2023 (2024 and 2022 are inactive)
            var returnedYears = new List<int>();
            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                returnedYears.Add(data[i].GetProperty("value").GetInt32());
            }

            Assert.Contains(2025, returnedYears);
            Assert.Contains(2023, returnedYears);
            Assert.DoesNotContain(2024, returnedYears);
            Assert.DoesNotContain(2022, returnedYears);
        }

        [Fact]
        public async Task GetYearMasterDropdown_VerifiesResponseStructure()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            // Verify top-level structure
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.True(value.TryGetProperty("data", out var data));

            // Verify item structure (value, text, yearStatus)
            var firstItem = data[0];
            Assert.True(firstItem.TryGetProperty("value", out _));
            Assert.True(firstItem.TryGetProperty("text", out _));
            Assert.True(firstItem.TryGetProperty("yearStatus", out _));
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithLargeDataset_ReturnsAllActiveYears()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>();
            for (int year = 2015; year <= 2025; year++)
            {
                yearMasterDtos.Add(new YearMasterDto
                {
                    FpsYear = year,
                    FpsYearCode = year.ToString(),
                    YearStatus = year == 2024 ? "Open" : "Closed",
                    Active = true
                });
            }

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            var data = value.GetProperty("data");
            Assert.Equal(11, data.GetArrayLength()); // All 11 years

            // Verify descending order (2025 first, 2015 last)
            Assert.Equal(2025, data[0].GetProperty("value").GetInt32());
            Assert.Equal(2015, data[10].GetProperty("value").GetInt32());
        }

        [Fact]
        public async Task GetYearMasterDropdown_EdgeCase_ServiceReturnsNull_ReturnsFailureJson()
        {
            // Arrange
            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse((IEnumerable<YearMasterDto>)null!);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve year masters", value.GetProperty("message").GetString());

            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API connection failed", Code = "API_CONN_FAIL" }
            };
            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.FailureResponse(errors, new ApiMetaDto());

            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve year masters", value.GetProperty("message").GetString());

            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_Error_ServiceThrows()
        {
            // Arrange
            _yearMasterService.GetAllFpsYearsAsync()
                .Throws(new InvalidOperationException("Service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetYearMasterDropdown()
            );

            Assert.Equal("Service error", exception.Message);
            await _yearMasterService.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithDifferentYearStatuses_ReturnsCorrectData()
        {
            // Arrange
            var yearMasterDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2026, FpsYearCode = "2026", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };

            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasterDtos);
            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            var data = value.GetProperty("data");
            Assert.Equal(4, data.GetArrayLength());

            // Verify each item has all required properties
            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                var item = data[i];
                Assert.True(item.TryGetProperty("value", out _));
                Assert.True(item.TryGetProperty("text", out _));
                Assert.True(item.TryGetProperty("yearStatus", out _));
            }
        }

        [Fact]
        public async Task GetYearMasterDropdown_WithSuccessButEmptyData_ReturnsSuccessWithEmptyArray()
        {
            // Arrange
            var emptyList = new List<YearMasterDto>();
            var serviceResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(emptyList);

            _yearMasterService.GetAllFpsYearsAsync().Returns(serviceResponse);

            // Act
            var result = await _controller.GetYearMasterDropdown();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);

            Assert.True(value.GetProperty("success").GetBoolean());
            var data = value.GetProperty("data");
            Assert.Equal(JsonValueKind.Array, data.ValueKind);
            Assert.Equal(0, data.GetArrayLength());
        }

        #endregion
    }
}
