using System.Text.Json;
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

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Controllers.AccountCategoryMaintenanceControllerTest
{
    public class AccountCategoryMaintenanceControllerTests
    {
        private const string TestAccShortName = "TEST001";
        private const string TestAccountDescription = "Test Description";
        private const string TestFilterType = "all";

        private readonly IMapper _mapper;
        private readonly IAccountCategoryService _accountCategoryService;
        private readonly AccountCategoryMaintenanceController _controller;

        public AccountCategoryMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _accountCategoryService = Substitute.For<IAccountCategoryService>();
            _controller = new AccountCategoryMaintenanceController(_mapper, _accountCategoryService);
        }

        // Helper method to extract properties from JsonResult
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class JsonResultSuccess
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public object? Data { get; set; }
        }

        private class JsonResultError
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public object? Errors { get; set; }
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithDefaultGrid()
        {
            // Arrange
            var pagedData = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = true,
                Data = new List<AccountCategoryDto>(),
                Pagination = new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };

            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(queryParameters);

            _accountCategoryService.GetFilteredAccountCategoriesAsync(queryParameters, TestFilterType)
                .Returns(pagedData);

            _mapper.Map<List<AccountCategoryViewModel>>(Arg.Any<List<AccountCategoryDto>>())
                .Returns(new List<AccountCategoryViewModel>());

            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { TotalRecords = 0, PageNumber = 1, PageSize = 10 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AccountCategoryMaintenanceViewModel>(viewResult.Model);
            Assert.NotNull(model.AccountCategoryGrid);
        }

        #endregion

        #region LoadAccountCategoryGrid

        [Fact]
        public async Task LoadAccountCategoryGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var pagedData = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = true,
                Data = new List<AccountCategoryDto>
                {
                    new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
                },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };

            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _accountCategoryService.GetFilteredAccountCategoriesAsync(queryParameters, TestFilterType)
                .Returns(pagedData);

            var viewModels = new List<AccountCategoryViewModel>
            {
                new AccountCategoryViewModel { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
            };
            _mapper.Map<List<AccountCategoryViewModel>>(Arg.Any<List<AccountCategoryDto>>()).Returns(viewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { TotalRecords = 1, PageNumber = 1, PageSize = 10 });

            // Act
            var result = await _controller.LoadAccountCategoryGrid(request, TestFilterType);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var config = Assert.IsType<DataGridConfig<AccountCategoryViewModel>>(partialViewResult.Model);
            Assert.Single(config.Data);
        }

        [Fact]
        public async Task LoadAccountCategoryGrid_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Required");

            // Act
            var result = await _controller.LoadAccountCategoryGrid(request, TestFilterType);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Message);
        }

        #endregion

        #region Create

        [Fact]
        public void Create_Get_ReturnsPartialView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAccountCategory", partialViewResult.ViewName);
        }

        [Fact]
        public async Task Create_Post_ValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };

            _mapper.Map<AccountCategoryDto>(viewModel).Returns(dto);
            _accountCategoryService.CreateAccountCategoryAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
            Assert.Equal("Account category created successfully", value.Message);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName };
            _controller.ModelState.AddModelError("AccountDescription", "Required");

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Errors);
        }

        [Fact]
        public async Task Create_Post_ServiceFailure_ReturnsJsonError()
        {
            // Arrange
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Creation failed" } }
            };

            _mapper.Map<AccountCategoryDto>(viewModel).Returns(dto);
            _accountCategoryService.CreateAccountCategoryAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Creation failed", value.Message);
        }

        #endregion

        #region Edit

        [Fact]
        public async Task Edit_Get_ExistingId_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };

            _accountCategoryService.GetAccountCategoryByIdAsync(TestAccShortName).Returns(apiResponse);
            _mapper.Map<AccountCategoryViewModel>(dto).Returns(viewModel);

            // Act
            var result = await _controller.Edit(TestAccShortName);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAccountCategory", partialViewResult.ViewName);
            var model = Assert.IsType<AccountCategoryViewModel>(partialViewResult.Model);
            Assert.Equal(TestAccShortName, model.AccShortName);
        }

        [Fact]
        public async Task Edit_Get_NullOrWhiteSpaceId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Account Short Name is required", value.Message);
        }

        [Fact]
        public async Task Edit_Get_NonExistingId_ReturnsJsonError()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found" } }
            };

            _accountCategoryService.GetAccountCategoryByIdAsync("NONEXISTENT").Returns(apiResponse);

            // Act
            var result = await _controller.Edit("NONEXISTENT");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Contains("not found", value.Message);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };

            _mapper.Map<AccountCategoryDto>(viewModel).Returns(dto);
            _accountCategoryService.UpdateAccountCategoryAsync(TestAccShortName, dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(viewModel, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
            Assert.Equal("Account category updated successfully", value.Message);
        }

        [Fact]
        public async Task Edit_Post_UsesOriginalAccShortNameWhenProvided()
        {
            // Arrange
            var originalId = "ORIGINAL";
            var viewModel = new AccountCategoryViewModel { AccShortName = "CHANGED", AccountDescription = "Updated" };
            var dto = new AccountCategoryDto { AccShortName = "CHANGED", AccountDescription = "Updated" };
            var apiResponse = new ApiResponseDto<AccountCategoryDto> { Success = true, Data = dto };

            _mapper.Map<AccountCategoryDto>(viewModel).Returns(dto);
            _accountCategoryService.UpdateAccountCategoryAsync(originalId, dto).Returns(apiResponse);

            // Act
            await _controller.Edit(viewModel, originalId);

            // Assert
            await _accountCategoryService.Received(1).UpdateAccountCategoryAsync(originalId, dto);
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName };
            _controller.ModelState.AddModelError("AccountDescription", "Required");

            // Act
            var result = await _controller.Edit(viewModel, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Errors);
        }

        [Fact]
        public async Task Edit_Post_ServiceFailure_ReturnsJsonError()
        {
            // Arrange
            var viewModel = new AccountCategoryViewModel { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed" } }
            };

            _mapper.Map<AccountCategoryDto>(viewModel).Returns(dto);
            _accountCategoryService.UpdateAccountCategoryAsync(TestAccShortName, dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(viewModel, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Update failed", value.Message);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ExistingId_ReturnsSuccessJson()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<bool> { Success = true };
            _accountCategoryService.DeleteAccountCategoryAsync(TestAccShortName).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(TestAccShortName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
            Assert.Equal("Account category deleted successfully", value.Message);
        }

        [Fact]
        public async Task Delete_NullOrWhiteSpaceId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Account Short Name is required", value.Message);
        }

        [Fact]
        public async Task Delete_ServiceFailure_ReturnsJsonError()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed" } }
            };
            _accountCategoryService.DeleteAccountCategoryAsync(TestAccShortName).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(TestAccShortName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Delete failed", value.Message);
        }

        #endregion

        #region GetAccountCategory

        [Fact]
        public async Task GetAccountCategory_ExistingId_ReturnsSuccessJson()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };
            _accountCategoryService.GetAccountCategoryByIdAsync(TestAccShortName).Returns(apiResponse);

            // Act
            var result = await _controller.GetAccountCategory(TestAccShortName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
            Assert.NotNull(value.Data);
        }

        [Fact]
        public async Task GetAccountCategory_NullOrWhiteSpaceId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.GetAccountCategory(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Account Short Name is required", value.Message);
        }

        [Fact]
        public async Task GetAccountCategory_ServiceFailure_ReturnsJsonError()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found" } }
            };
            _accountCategoryService.GetAccountCategoryByIdAsync(TestAccShortName).Returns(apiResponse);

            // Act
            var result = await _controller.GetAccountCategory(TestAccShortName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Errors);
        }

        #endregion
    }
}
