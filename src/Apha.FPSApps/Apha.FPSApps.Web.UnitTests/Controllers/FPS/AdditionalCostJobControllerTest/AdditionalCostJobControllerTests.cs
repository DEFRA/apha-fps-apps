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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.AdditionalCostJobControllerTest
{
    public class AdditionalCostJobControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IAdditionalCostService _additionalCostService;
        private readonly AdditionalCostJobController _controller;

        public AdditionalCostJobControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _additionalCostService = Substitute.For<IAdditionalCostService>();
            _controller = new AdditionalCostJobController(_mapper, _additionalCostService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static AdditionalCostDto BuildDto(string jobCode = "JOB001") =>
            new() { JobCode = jobCode, Account = "ACC001", Description = "Test Cost", ItemCost = 100m };

        private static AdditionalCostItemViewModel BuildViewModel(string jobCode = "JOB001") =>
            new() { JobCode = jobCode, Account = "ACC001", Description = "Test Cost", ItemCost = 100m };

        #region LoadAdditionalCostGrid Tests

        [Fact]
        public async Task LoadAdditionalCostGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var costs = new List<AdditionalCostDto>
            {
                new() { JobCode = jobCode, Account = "ACC001", Description = "Cost A", ItemCost = 50m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(costs, paginationDto);
            var viewItems = new List<AdditionalCostItemViewModel> { new() { Account = "ACC001", Description = "Cost A" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _additionalCostService.GetAdditionalCostsAsync(queryParameters, jobCode).Returns(serviceResponse);
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>()).Returns(viewItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadAdditionalCostGrid(request, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostItemViewModel>>(partialView.Model);
            Assert.Equal("additionalCostGrid", gridConfig.GridId);
            Assert.Equal("Additional Cost Plan", gridConfig.Title);
            Assert.Equal("Description", gridConfig.KeyProperty);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadAdditionalCostGrid(request, "JOB001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _additionalCostService.DidNotReceive().GetAdditionalCostsAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WithNullJobCode_UsesEmptyString()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                new List<AdditionalCostDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _additionalCostService.GetAdditionalCostsAsync(queryParameters, string.Empty).Returns(serviceResponse);
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>()).Returns(new List<AdditionalCostItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAdditionalCostGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _additionalCostService.Received(1).GetAdditionalCostsAsync(queryParameters, string.Empty);
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WhenServiceReturnsNullData_MapsEmptyList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _additionalCostService.GetAdditionalCostsAsync(queryParameters, jobCode).Returns(serviceResponse);
            _mapper.Map<PaginationModel>(Arg.Any<object?>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAdditionalCostGrid(request, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostItemViewModel>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_WithJobCode_ReturnsPartialViewWithModel()
        {
            // Arrange
            var jobCode = "JOB001";
            var categories = new List<AccountCategoryDto>
            {
                new() { AccShortName = "ACC001", AccountDescription = "Travel" }
            };
            _additionalCostService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(categories));

            // Act
            var result = await _controller.Create(jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAdditionalCostJob", partialView.ViewName);
            var model = Assert.IsType<AdditionalCostItemViewModel>(partialView.Model);
            Assert.Equal(jobCode, model.JobCode);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var viewModel = BuildViewModel();
            var dto = BuildDto();
            var serviceResponse = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);

            _mapper.Map<AdditionalCostDto>(viewModel).Returns(dto);
            _additionalCostService.CreateAdditionalCostAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Additional cost created successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Description", "Description is required");
            var viewModel = BuildViewModel();

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the validation errors.", value.GetProperty("message").GetString());
            await _additionalCostService.DidNotReceive().CreateAdditionalCostAsync(Arg.Any<AdditionalCostDto>());
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var viewModel = BuildViewModel();
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate entry", Code = "DUPLICATE" } };
            var serviceResponse = ApiResponseDto<AdditionalCostDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<AdditionalCostDto>(viewModel).Returns(dto);
            _additionalCostService.CreateAdditionalCostAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Duplicate entry", value.GetProperty("message").GetString());
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_WithValidKeys_ReturnsPartialViewWithModel()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var description = "Test Cost";
            var dto = BuildDto(jobCode);
            var viewModel = BuildViewModel(jobCode);
            var categories = new List<AccountCategoryDto> { new() { AccShortName = "ACC001", AccountDescription = "Travel" } };

            _additionalCostService.GetByIdAsync(jobCode, account, description)
                .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto));
            _mapper.Map<AdditionalCostItemViewModel>(dto).Returns(viewModel);
            _additionalCostService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(categories));

            // Act
            var result = await _controller.Edit(jobCode, account, description);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAdditionalCostJob", partialView.ViewName);
            var model = Assert.IsType<AdditionalCostItemViewModel>(partialView.Model);
            Assert.Equal(jobCode, model.JobCode);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            _additionalCostService.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<AdditionalCostDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit("JOB001", "ACC001", "Missing");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve additional cost details.", value.GetProperty("message").GetString());
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var viewModel = BuildViewModel(jobCode);
            var dto = BuildDto(jobCode);
            var serviceResponse = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);

            _mapper.Map<AdditionalCostDto>(viewModel).Returns(dto);
            _additionalCostService.UpdateAdditionalCostAsync(jobCode, account, dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(jobCode, account, viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Additional cost updated successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("ItemCost", "Item cost is required");
            var viewModel = BuildViewModel();

            // Act
            var result = await _controller.Edit("JOB001", "ACC001", viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the validation errors.", value.GetProperty("message").GetString());
            await _additionalCostService.DidNotReceive().UpdateAdditionalCostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AdditionalCostDto>());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var viewModel = BuildViewModel(jobCode);
            var dto = BuildDto(jobCode);
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var serviceResponse = ApiResponseDto<AdditionalCostDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<AdditionalCostDto>(viewModel).Returns(dto);
            _additionalCostService.UpdateAdditionalCostAsync(jobCode, account, dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(jobCode, account, viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Update failed", value.GetProperty("message").GetString());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidKeys_ReturnsSuccessJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var description = "Test Cost";
            _additionalCostService.DeleteAdditionalCostAsync(Arg.Any<AdditionalCostDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.Delete(jobCode, account, description);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Additional cost deleted successfully.", value.GetProperty("message").GetString());
            await _additionalCostService.Received(1).DeleteAdditionalCostAsync(
                Arg.Is<AdditionalCostDto>(d => d.JobCode == jobCode && d.Account == account && d.Description == description));
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            _additionalCostService.DeleteAdditionalCostAsync(Arg.Any<AdditionalCostDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Delete("JOB001", "ACC001", "Test Cost");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Delete failed", value.GetProperty("message").GetString());
        }

        #endregion

        #region GetTotalItemCost Tests

        [Fact]
        public async Task GetTotalItemCost_WithValidJobCode_ReturnsTotalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            _additionalCostService.GetTotalItemCostAsync(jobCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(350m));

            // Act
            var result = await _controller.GetTotalItemCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(350m, value.GetProperty("totalItemCost").GetDecimal());
            await _additionalCostService.Received(1).GetTotalItemCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalItemCost_WithNullJobCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalItemCost(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job code is required.", value.GetProperty("message").GetString());
            await _additionalCostService.DidNotReceive().GetTotalItemCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalItemCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            _additionalCostService.GetTotalItemCostAsync(jobCode)
                .Returns(ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTotalItemCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve total item cost.", value.GetProperty("message").GetString());
            Assert.Equal(0, value.GetProperty("totalItemCost").GetDecimal());
        }

        #endregion
    }
}
