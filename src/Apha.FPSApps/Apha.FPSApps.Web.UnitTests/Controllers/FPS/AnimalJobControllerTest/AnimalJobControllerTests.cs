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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.AnimalJobControllerTest
{
    public class AnimalJobControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly AnimalJobController _controller;

        public AnimalJobControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _animalPlanService = Substitute.For<IAnimalPlanService>();
            _controller = new AnimalJobController(_mapper, _animalPlanService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region LoadAnimalPlanGrid Tests

        [Fact]
        public async Task LoadAnimalPlanGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalCosts = new List<AnimalCostViewDto>
            {
                new() { IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode, NumberOfDays = 5 }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(animalCosts, paginationDto);
            var animalItems = new List<AnimalPlanItem> { new() { AnimalType = "Cattle" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _animalPlanService.GetAllAnimalCostAsync(queryParameters, jobCode).Returns(serviceResponse);
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>()).Returns(animalItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadAnimalPlanGrid(request, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalPlanItem>>(partialView.Model);
            Assert.Equal("animalBookedGrid", gridConfig.GridId);
            Assert.Equal("Animal Plan",      gridConfig.Title);
            Assert.Equal("IndCounter",       gridConfig.KeyProperty);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadAnimalPlanGrid(request, "JOB001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _animalPlanService.DidNotReceive().GetAllAnimalCostAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WithNullJobCode_UsesEmptyString()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                new List<AnimalCostViewDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _animalPlanService.GetAllAnimalCostAsync(queryParameters, string.Empty).Returns(serviceResponse);
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>()).Returns(new List<AnimalPlanItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAnimalPlanGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _animalPlanService.Received(1).GetAllAnimalCostAsync(queryParameters, string.Empty);
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WhenServiceReturnsNullData_MapsEmptyAnimalList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<AnimalCostViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _animalPlanService.GetAllAnimalCostAsync(queryParameters, jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadAnimalPlanGrid(request, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalPlanItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>());
        }

        #endregion

        #region Create (GET) Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialView_WithPopulatedAnimalDropdown()
        {
            // Arrange
            var animals = new List<AnimalDto>
            {
                new() { AnimalType = "Cattle", DailyRate = 25m },
                new() { AnimalType = "Sheep",  DailyRate = 15m }
            };
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.SuccessResponse(animals));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalPlan", partialView.ViewName);
            var model = Assert.IsType<AnimalPlanItem>(partialView.Model);
            Assert.Equal(2, model.AnimalTypeList.Count);
            Assert.Equal("Cattle", model.AnimalTypeList[0].Value);
        }

        [Fact]
        public async Task Create_Get_PopulatesAnimalDropdown_SortedAlphabetically()
        {
            // Arrange
            var animals = new List<AnimalDto>
            {
                new() { AnimalType = "Zebra", DailyRate = 10m },
                new() { AnimalType = "TEST", DailyRate = 10m },
                new() { AnimalType = "B&B Fixed Price, Avian", DailyRate = 10m },
                new() { AnimalType = "Sheep, High Security", DailyRate = 15m }
            };
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.SuccessResponse(animals));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AnimalPlanItem>(partialView.Model);
            Assert.Equal(
                new[] { "B&B Fixed Price, Avian", "Sheep, High Security", "TEST", "Zebra" },
                model.AnimalTypeList.Select(x => x.Value));
        }

        [Fact]
        public async Task Create_Get_WhenAnimalLookupFails_ReturnsEmptyDropdown()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AnimalPlanItem>(partialView.Model);
            Assert.Empty(model.AnimalTypeList);
        }

        #endregion

        #region Create (POST) Tests

        [Fact]
        public async Task Create_Post_WithValidRequest_ReturnsSuccessJson()
        {
            // Arrange
            var animalPlanItem = new AnimalPlanItem { JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 5, NumberOfAnimals = 10 };
            var animalRequestDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 5, NumberOfAnimals = 10 };
            var serviceResponse = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequestDto);

            _mapper.Map<AnimalRequestDto>(animalPlanItem).Returns(animalRequestDto);
            _animalPlanService.CreateAnimalCostAsync(animalRequestDto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(animalPlanItem);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal cost created successfully", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("AnimalType", "Animal type is required");
            var animalPlanItem = new AnimalPlanItem();

            // Act
            var result = await _controller.Create(animalPlanItem);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            await _animalPlanService.DidNotReceive().CreateAnimalCostAsync(Arg.Any<AnimalRequestDto>());
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var animalPlanItem = new AnimalPlanItem { JobCode = "JOB001", AnimalType = "Cattle" };
            var animalRequestDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "Cattle" };
            var errors = new List<ApiErrorDto> { new() { Message = "Failed to save", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<AnimalRequestDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<AnimalRequestDto>(animalPlanItem).Returns(animalRequestDto);
            _animalPlanService.CreateAnimalCostAsync(animalRequestDto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(animalPlanItem);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to save", value.GetProperty("message").GetString());
        }

        #endregion

        #region Edit (GET) Tests

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsPartialViewWithModel()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";
            var costViewDto = new AnimalCostViewDto { IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode };
            var serviceResponse = ApiResponseDto<AnimalCostViewDto?>.SuccessResponse(costViewDto);
            var animalPlanItem = new AnimalPlanItem { AnimalType = "Cattle" };
            var animals = new List<AnimalDto> { new() { AnimalType = "Cattle" } };

            _animalPlanService.GetAnimalCostViewByIdAsync(indCounter, jobCode).Returns(serviceResponse);
            _mapper.Map<AnimalPlanItem>(costViewDto).Returns(animalPlanItem);
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.SuccessResponse(animals));

            // Act
            var result = await _controller.Edit(indCounter, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalPlan", partialView.ViewName);
            Assert.IsType<AnimalPlanItem>(partialView.Model);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsFailureJson()
        {
            // Arrange
            var indCounter = 999;
            var serviceResponse = ApiResponseDto<AnimalCostViewDto?>.SuccessResponse(null);
            _animalPlanService.GetAnimalCostViewByIdAsync(indCounter, Arg.Any<string>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(indCounter, "JOB001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve animal cost details.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_Get_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<AnimalCostViewDto?>.FailureResponse(errors, new ApiMetaDto());
            _animalPlanService.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(1, "JOB001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Edit (POST) Tests

        [Fact]
        public async Task Edit_Post_WithValidRequest_ReturnsSuccessJson()
        {
            // Arrange
            var indCounter = 1;
            var animalPlanItem = new AnimalPlanItem { AnimalType = "Cattle", NumberOfDays = 7, NumberOfAnimals = 5 };
            var animalRequestDto = new AnimalRequestDto { AnimalType = "Cattle", NumberOfDays = 7, NumberOfAnimals = 5 };
            var serviceResponse = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequestDto);

            _mapper.Map<AnimalRequestDto>(animalPlanItem).Returns(animalRequestDto);
            _animalPlanService.UpdateAnimalCostAsync(Arg.Any<AnimalRequestDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(indCounter, animalPlanItem);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal cost updated successfully", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_Post_SetsIndCounterOnDto_BeforeCallingService()
        {
            // Arrange
            var indCounter = 42;
            var animalPlanItem = new AnimalPlanItem { AnimalType = "Cattle" };
            var animalRequestDto = new AnimalRequestDto { AnimalType = "Cattle" };
            var serviceResponse = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequestDto);

            _mapper.Map<AnimalRequestDto>(animalPlanItem).Returns(animalRequestDto);
            _animalPlanService.UpdateAnimalCostAsync(Arg.Any<AnimalRequestDto>()).Returns(serviceResponse);

            // Act
            await _controller.Edit(indCounter, animalPlanItem);

            // Assert — dto.IndCounter was set to indCounter before service call
            await _animalPlanService.Received(1).UpdateAnimalCostAsync(
                Arg.Is<AnimalRequestDto>(dto => dto.IndCounter == indCounter));
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("NumberOfDays", "Days must be positive");
            var animalPlanItem = new AnimalPlanItem();

            // Act
            var result = await _controller.Edit(1, animalPlanItem);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            await _animalPlanService.DidNotReceive().UpdateAnimalCostAsync(Arg.Any<AnimalRequestDto>());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var animalPlanItem = new AnimalPlanItem { AnimalType = "Cattle" };
            var animalRequestDto = new AnimalRequestDto { AnimalType = "Cattle" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<AnimalRequestDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<AnimalRequestDto>(animalPlanItem).Returns(animalRequestDto);
            _animalPlanService.UpdateAnimalCostAsync(Arg.Any<AnimalRequestDto>()).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(1, animalPlanItem);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Not found", value.GetProperty("message").GetString());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidIndCounter_ReturnsSuccessJson()
        {
            // Arrange
            var serviceResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _animalPlanService.DeleteAnimalCostAsync(1).Returns(serviceResponse);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal cost deleted successfully", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _animalPlanService.DeleteAnimalCostAsync(999).Returns(serviceResponse);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Not found", value.GetProperty("message").GetString());
        }

        #endregion

        #region GetAnimalRate Tests

        [Fact]
        public async Task GetAnimalRate_WithValidAnimalType_ReturnsSuccessJson()
        {
            // Arrange
            var animalType = "Cattle";
            var jobCode = "JOB001";
            var serviceResponse = ApiResponseDto<decimal?>.SuccessResponse(25.50m);
            _animalPlanService.GetAnimalRateAsync(animalType, jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetAnimalRate(animalType, jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(25.50m, value.GetProperty("dailyRate").GetDecimal());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetAnimalRate_WithEmptyOrWhitespaceAnimalType_ReturnsFailureJson(string animalType)
        {
            // Act
            var result = await _controller.GetAnimalRate(animalType, "JOB001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Animal type is required", value.GetProperty("message").GetString());
            await _animalPlanService.DidNotReceive().GetAnimalRateAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetAnimalRate_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<decimal?>.FailureResponse(errors, new ApiMetaDto());
            _animalPlanService.GetAnimalRateAsync("Cattle", jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetAnimalRate("Cattle", jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve animal rate.", value.GetProperty("message").GetString());
            Assert.Equal(0m, value.GetProperty("dailyRate").GetDecimal());
        }

        #endregion

        #region GetTotalAnimalCost Tests

        [Fact]
        public async Task GetTotalAnimalCost_WithValidJobCode_ReturnsSuccessJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var serviceResponse = ApiResponseDto<decimal>.SuccessResponse(4500.00m);
            _animalPlanService.GetTotalAnimalCostAsync(jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalAnimalCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(4500.00m, value.GetProperty("totalAnimalCost").GetDecimal());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalAnimalCost_WithEmptyOrWhitespaceJobCode_ReturnsFailureJson(string jobCode)
        {
            // Act
            var result = await _controller.GetTotalAnimalCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required", value.GetProperty("message").GetString());
            await _animalPlanService.DidNotReceive().GetTotalAnimalCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalAnimalCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _animalPlanService.GetTotalAnimalCostAsync("JOB001").Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalAnimalCost("JOB001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve total animal cost.", value.GetProperty("message").GetString());
            Assert.Equal(0m, value.GetProperty("totalAnimalCost").GetDecimal());
        }

        #endregion
    }
}
