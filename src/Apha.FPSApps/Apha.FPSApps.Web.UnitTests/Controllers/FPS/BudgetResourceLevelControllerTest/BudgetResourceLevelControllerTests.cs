using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BudgetResourceLevelControllerTest
{
    public class BudgetResourceLevelControllerTests
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IPurchasesService _purchasesService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly BudgetResourceLevelController _controller;

        public BudgetResourceLevelControllerTests()
        {
            _workGroupService    = Substitute.For<IWorkGroupService>();
            _budgetBidsService   = Substitute.For<IBudgetBidsService>();
            _purchasesService    = Substitute.For<IPurchasesService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();

            _controller = new BudgetResourceLevelController(
                _workGroupService,
                _budgetBidsService,
                _purchasesService,
                _profitCentreService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private void SetupDefaultProfitCentres()
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>
                {
                    new() { ProfitCentreId = "PC01", ProfitCentreName = "PC 01" }
                }));
        }

        private void SetupDefaultWorkGroups(string profitCentre = "PC01")
        {
            _workGroupService.GetWorkGroupsAsync(profitCentre)
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkgroupName = "WG01", ProfitCentre = profitCentre }
                }));
        }

        private void SetupDefaultBidView(string workgroup = "WG01")
        {
            _budgetBidsService.GetBidViewAsync(workgroup)
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>
                {
                    new() { WorkgroupName = workgroup, Account = "ACC1", GenBid = 100m }
                }));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>
                {
                    new() { AccShortName = "ACC1", AccountDescription = "Account 1" }
                }));
        }

        private void SetupDefaultPurchases(string workgroup = "WG01", string account = "ACC1")
        {
            _purchasesService.GetPurchasesAsync(workgroup, account)
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>
                {
                    new() { WorkgroupName = workgroup, Account = account, ItemDescription = "Item A", Amount = 50m }
                }));
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithNoProfitCentre_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDefaultProfitCentres();
            _workGroupService.GetWorkGroupsAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>()));
            _budgetBidsService.GetBidViewAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
            _purchasesService.GetPurchasesAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BudgetResourceLevelViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProfitCentre);
            Assert.NotNull(model.WorkGroupGrid);
            Assert.NotNull(model.BudgetBidsGrid);
            Assert.NotNull(model.PurchasesGrid);
        }

        [Fact]
        public async Task Index_WithProfitCentre_LoadsWorkGroupGrid()
        {
            // Arrange
            SetupDefaultProfitCentres();
            SetupDefaultWorkGroups("PC01");
            _budgetBidsService.GetBidViewAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
            _purchasesService.GetPurchasesAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>()));

            // Act
            var result = await _controller.Index("PC01");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BudgetResourceLevelViewModel>(viewResult.Model);
            Assert.Equal("PC01", model.SelectedProfitCentre);
            Assert.Equal("workGroupGrid", model.WorkGroupGrid.GridId);
        }

        [Fact]
        public async Task Index_SetsGridIds_Correctly()
        {
            // Arrange
            SetupDefaultProfitCentres();
            _workGroupService.GetWorkGroupsAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>()));
            _budgetBidsService.GetBidViewAsync(Arg.Any<string>())
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
            _purchasesService.GetPurchasesAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BudgetResourceLevelViewModel>(viewResult.Model);
            Assert.Equal("workGroupGrid",   model.WorkGroupGrid.GridId);
            Assert.Equal("budgetBidsGrid",  model.BudgetBidsGrid.GridId);
            Assert.Equal("purchasesGrid",   model.PurchasesGrid.GridId);
        }

        #endregion

        #region LoadWorkGroupGrid Tests

        [Fact]
        public async Task LoadWorkGroupGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultWorkGroups("PC01");

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, "PC01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.LoadWorkGroupGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WithNullProfitCentre_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
            Assert.Empty(config.Data!);
        }

        [Fact]
        public async Task LoadWorkGroupGrid_WhenServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            _workGroupService.GetWorkGroupsAsync("PC01")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadWorkGroupGrid(request, "PC01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WorkGroupItem>>(partialView.Model);
            Assert.Empty(config.Data!);
        }

        #endregion

        #region LoadBudgetBidsGrid Tests

        [Fact]
        public async Task LoadBudgetBidsGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultBidView("WG01");

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request, "WG01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<BudgetResourceCentreLevelItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadBudgetBidsGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadBudgetBidsGrid_WithNullWorkgroup_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<BudgetResourceCentreLevelItem>>(partialView.Model);
            Assert.Empty(config.Data!);
        }

        [Fact]
        public async Task LoadBudgetBidsGrid_GridConfig_HasCorrectSettings()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultBidView("WG01");

            // Act
            var result = await _controller.LoadBudgetBidsGrid(request, "WG01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<BudgetResourceCentreLevelItem>>(partialView.Model);
            Assert.Equal("budgetBidsGrid", config.GridId);
            Assert.True(config.AllowAdd);
            Assert.True(config.AllowEdit);
            Assert.True(config.AllowDelete);
        }

        #endregion

        #region LoadPurchasesGrid Tests

        [Fact]
        public async Task LoadPurchasesGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultPurchases("WG01", "ACC1");

            // Act
            var result = await _controller.LoadPurchasesGrid(request, "WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<PurchaseItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadPurchasesGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.LoadPurchasesGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadPurchasesGrid_WithNullWorkgroupOrAccount_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            // Act - null workgroup
            var result1 = await _controller.LoadPurchasesGrid(request, null, "ACC1");
            var config1 = Assert.IsType<DataGridConfig<PurchaseItem>>(Assert.IsType<PartialViewResult>(result1).Model);
            Assert.Empty(config1.Data!);

            // Act - null account
            var result2 = await _controller.LoadPurchasesGrid(request, "WG01", null);
            var config2 = Assert.IsType<DataGridConfig<PurchaseItem>>(Assert.IsType<PartialViewResult>(result2).Model);
            Assert.Empty(config2.Data!);
        }

        #endregion

        #region CreateBudgetBid Tests

        [Fact]
        public async Task CreateBudgetBid_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>
                {
                    new() { AccShortName = "ACC1", AccountDescription = "Account 1" }
                }));

            // Act
            var result = await _controller.CreateBudgetBid("WG01");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
            var model = Assert.IsType<BudgetResourceLevelItem>(partialView.Model);
            Assert.Equal(BudgetResourceLevelModalType.BudgetBid, model.ModalType);
        }

        [Fact]
        public async Task CreateBudgetBid_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var dto   = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _budgetBidsService.CreateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Budget bid created successfully.", value.message);
        }

        [Fact]
        public async Task CreateBudgetBid_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem();
            _controller.ModelState.AddModelError("Account", "Account is required");

            // Act
            var result = await _controller.CreateBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the validation errors.", value.message);
        }

        [Fact]
        public async Task CreateBudgetBid_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _budgetBidsService.CreateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to create bid.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.CreateBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to create bid.", value.message);
        }

        #endregion

        #region EditBudgetBid Tests

        [Fact]
        public async Task EditBudgetBid_Get_WithExistingBid_ReturnsPartialView()
        {
            // Arrange
            var dto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _budgetBidsService.GetBidByIdAsync("WG01", "ACC1")
                .Returns(ApiResponseDto<BidDto>.SuccessResponse(dto));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            // Act
            var result = await _controller.EditBudgetBid("WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
        }

        [Fact]
        public async Task EditBudgetBid_Get_WhenBidNotFound_ReturnsJsonError()
        {
            // Arrange
            _budgetBidsService.GetBidByIdAsync("WG01", "NOTEXIST")
                .Returns(ApiResponseDto<BidDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.EditBudgetBid("WG01", "NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditBudgetBid_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var dto   = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            _budgetBidsService.UpdateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Budget bid updated successfully.", value.message);
        }

        [Fact]
        public async Task EditBudgetBid_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem();
            _controller.ModelState.AddModelError("Account", "Account is required");

            // Act
            var result = await _controller.EditBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditBudgetBid_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new BudgetResourceCentreLevelItem { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            _budgetBidsService.UpdateBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<BidDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to update bid.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.EditBudgetBid(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to update bid.", value.message);
        }

        #endregion

        #region DeleteBudgetBid Tests

        [Fact]
        public async Task DeleteBudgetBid_WithExistingBid_ReturnsSuccessJson()
        {
            // Arrange
            _budgetBidsService.DeleteBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteBudgetBid("WG01", "ACC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Budget bid deleted successfully.", value.message);
        }

        [Fact]
        public async Task DeleteBudgetBid_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            _budgetBidsService.DeleteBidAsync(Arg.Any<BidDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to delete budget bid.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteBudgetBid("WG01", "ACC1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region CreatePurchase Tests

        [Fact]
        public void CreatePurchase_Get_ReturnsPartialViewWithModel()
        {
            // Act
            var result = _controller.CreatePurchase("WG01", "ACC1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
            var model = Assert.IsType<BudgetResourceLevelItem>(partialView.Model);
            Assert.Equal(BudgetResourceLevelModalType.Purchase, model.ModalType);
            Assert.Equal("WG01", model.Purchase!.WorkgroupName);
            Assert.Equal("ACC1", model.Purchase.Account);
        }

        [Fact]
        public async Task CreatePurchase_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new PurchaseItem { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var dto   = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _purchasesService.CreatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreatePurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Purchase created successfully.", value.message);
        }

        [Fact]
        public async Task CreatePurchase_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem();
            _controller.ModelState.AddModelError("ItemDescription", "Item Description is required");

            // Act
            var result = await _controller.CreatePurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task CreatePurchase_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _purchasesService.CreatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to create purchase.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.CreatePurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to create purchase.", value.message);
        }

        #endregion

        #region EditPurchase Tests

        [Fact]
        public async Task EditPurchase_Get_WithExistingPurchase_ReturnsPartialView()
        {
            // Arrange
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _purchasesService.GetPurchaseByIdAsync("WG01", "ACC1", "Item A")
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditPurchase("WG01", "ACC1", "Item A");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditBudgetResourceLevel", partialView.ViewName);
        }

        [Fact]
        public async Task EditPurchase_Get_WhenPurchaseNotFound_ReturnsJsonError()
        {
            // Arrange
            _purchasesService.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST")
                .Returns(ApiResponseDto<PurchaseDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto()));

            // Act
            var result = await _controller.EditPurchase("WG01", "ACC1", "NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditPurchase_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new PurchaseItem { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var dto   = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            _purchasesService.UpdatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditPurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Purchase updated successfully.", value.message);
        }

        [Fact]
        public async Task EditPurchase_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem();
            _controller.ModelState.AddModelError("ItemDescription", "Item Description is required");

            // Act
            var result = await _controller.EditPurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task EditPurchase_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var model = new PurchaseItem { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            _purchasesService.UpdatePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<PurchaseDto>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to update purchase.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.EditPurchase(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region DeletePurchase Tests

        [Fact]
        public async Task DeletePurchase_WithExistingPurchase_ReturnsSuccessJson()
        {
            // Arrange
            _purchasesService.DeletePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeletePurchase("WG01", "ACC1", "Item A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Purchase deleted successfully.", value.message);
        }

        [Fact]
        public async Task DeletePurchase_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            _purchasesService.DeletePurchaseAsync(Arg.Any<PurchaseDto>())
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Failed to delete purchase.", Code = "ERR" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.DeletePurchase("WG01", "ACC1", "Item A");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region ExportToExcel Tests

        [Fact]
        public async Task ExportToExcel_WithWorkgroupsAndBids_ReturnsExcelFile()
        {
            // Arrange
            SetupDefaultWorkGroups("PC01");
            SetupDefaultBidView("WG01");

            // Act
            var result = await _controller.ExportToExcel("PC01");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal("qryBidxCrosstab.xlsx", fileResult.FileDownloadName);
            Assert.NotEmpty(fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToExcel_WhenWorkgroupServiceFails_ReturnsEmptyExcel()
        {
            // Arrange
            _workGroupService.GetWorkGroupsAsync("PC01")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                    new List<AccountCategoryDto> { new() { AccShortName = "ACC1" } }));

            // Act
            var result = await _controller.ExportToExcel("PC01");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotEmpty(fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToExcel_WhenNoBids_ReturnsExcelWithHeadersOnly()
        {
            // Arrange
            SetupDefaultWorkGroups("PC01");
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>
                {
                    new() { AccShortName = "ACC1" }
                }));
            _budgetBidsService.GetBidViewAsync("WG01")
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>()));

            // Act
            var result = await _controller.ExportToExcel("PC01");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotEmpty(fileResult.FileContents);
        }

        #endregion
    }

    // Local helper record for deserialising JsonResult values
    internal record JsonResponse(bool success, string message);
}
