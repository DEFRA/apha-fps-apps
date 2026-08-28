using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PactMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.PactMonthlyOutputDto;
using StagingMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.StagingMonthlyOutputDto;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.MonthlyOutputControllerTest
{
    public class MonthlyOutputControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _monthlyOutputService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly MonthlyOutputController _controller;

        public MonthlyOutputControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _monthlyOutputService = Substitute.For<IPactMonthlyOutputService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _monthService = Substitute.For<IMonthService>();
            _excelExportService = Substitute.For<IExcelExportService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();

            _controller = new MonthlyOutputController(
                _mapper,
                _monthlyOutputService,
                _workGroupService,
                _monthService,
                _excelExportService,
                _testCapabilityService,
                _testRequirementService);

            SetupDefaultServiceMocks();
        }

        private void SetupDefaultServiceMocks()
        {
            _workGroupService.GetAllWorkGroupsAsync().Returns(
                ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                [
                    new WorkGroupDto { WorkGroupName = "WG1" }
                ]));
            _monthService.GetAllMonthsAsync().Returns(
                ApiResponseDto<List<MonthDto>>.SuccessResponse(
                [
                    new MonthDto { Monthnumber = 6, Monthname = "June" }
                ]));
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyOutputDto>>.SuccessResponse([]));
            _monthlyOutputService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(ApiResponseDto<List<PactMonthlyOutputDto>>.SuccessResponse([]));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<MonthlyOutputLiveItem>>(Arg.Any<List<PactMonthlyOutputDto>>())
                .Returns([]);
            _mapper.Map<List<StagingMonthlyOutputItem>>(Arg.Any<List<StagingMonthlyOutputDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]));
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputViewModel>(viewResult.Model);
            Assert.NotNull(model.LiveGrid);
            Assert.NotNull(model.StagingGrid);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.NotNull(model.MonthOptions);
        }

        [Fact]
        public async Task Index_WhenWorkGroupServiceFails_ReturnsEmptyOptions()
        {
            _workGroupService.GetAllWorkGroupsAsync().Returns(
                ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WhenMonthServiceFails_ReturnsEmptyMonthOptions()
        {
            _monthService.GetAllMonthsAsync().Returns(
                ApiResponseDto<List<MonthDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputViewModel>(viewResult.Model);
            Assert.Empty(model.MonthOptions);
        }

        #endregion

        #region LoadLiveGrid Tests

        [Fact]
        public async Task LoadLiveGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("WorkGroup", "Required");

            var result = await _controller.LoadLiveGrid(new PaginationFilter<string> { Filter = "{}" }, "WG1", "TC1", "B1", 6);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadLiveGrid_WithFilters_ReturnsPartialView()
        {
            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", "TC1", "B1", 6);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadLiveGrid_WithNoFilters_ReturnsPartialViewWithEmptyData()
        {
            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                null, null, null, null);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadLiveGrid_WhenServiceFails_ReturnsPartialViewWithEmptyItems()
        {
            _monthlyOutputService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(ApiResponseDto<List<PactMonthlyOutputDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", null, null, null);

            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadLiveGrid_WithPaginationResponse_MapsPagination()
        {
            var paginationDto = new PaginationDto { TotalRecords = 100, PageNumber = 1, PageSize = 10 };
            _monthlyOutputService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(new ApiResponseDto<List<PactMonthlyOutputDto>>
                {
                    Success = true,
                    Data = [],
                    Pagination = paginationDto,
                    Total = 500
                });

            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", null, null, null);

            Assert.IsType<PartialViewResult>(result);
            _mapper.Received().Map<PaginationModel>(paginationDto);
        }

        [Fact]
        public async Task LoadLiveGrid_WithNullPagination_UsesDefaultPagination()
        {
            _monthlyOutputService.GetLiveAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .Returns(new ApiResponseDto<List<PactMonthlyOutputDto>>
                {
                    Success = true,
                    Data = [],
                    Pagination = null,
                    Total = 0
                });

            var result = await _controller.LoadLiveGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                "WG1", null, null, null);

            Assert.IsType<PartialViewResult>(result);
        }

        #endregion

        #region LoadStagingGrid Tests

        [Fact]
        public async Task LoadStagingGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Passed", "Invalid");

            var result = await _controller.LoadStagingGrid(new PaginationFilter<string> { Filter = "{}" }, true);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadStagingGrid_ValidRequest_ReturnsPartialView()
        {
            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, true);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadStagingGrid_WhenServiceFails_ReturnsPartialViewWithEmptyItems()
        {
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyOutputDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);

            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadStagingGrid_WithPaginationResponse_MapsPagination()
        {
            var paginationDto = new PaginationDto { TotalRecords = 50 };
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(new ApiResponseDto<List<StagingMonthlyOutputDto>>
                {
                    Success = true,
                    Data = [],
                    Pagination = paginationDto,
                    Total = 200
                });

            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);

            Assert.IsType<PartialViewResult>(result);
            _mapper.Received().Map<PaginationModel>(paginationDto);
        }

        [Fact]
        public async Task LoadStagingGrid_WithNullPagination_UsesDefaultPagination()
        {
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(new ApiResponseDto<List<StagingMonthlyOutputDto>>
                {
                    Success = true,
                    Data = [],
                    Pagination = null,
                    Total = 0
                });

            var result = await _controller.LoadStagingGrid(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);

            Assert.IsType<PartialViewResult>(result);
        }

        #endregion

        #region GetTestCodesByWorkGroup Tests

        [Fact]
        public async Task GetTestCodesByWorkGroup_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetTestCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetTestCodesByWorkGroup_WhenServiceSucceeds_ReturnsDistinctTestCodes()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                [
                    new TestCapabilityDto { TestCode = "TC2" },
                    new TestCapabilityDto { TestCode = "TC1" },
                    new TestCapabilityDto { TestCode = "TC2" }
                ]));

            var result = await _controller.GetTestCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        [Fact]
        public async Task GetTestCodesByWorkGroup_WithBlankWorkGroup_PassesNullToService()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));

            await _controller.GetTestCodesByWorkGroup("  ");

            await _testCapabilityService.Received(1).GetPagedByWorkGroupAsync(
                Arg.Any<QueryParameters<string>>(), null);
        }

        [Fact]
        public async Task GetTestCodesByWorkGroup_FiltersBlankTestCodes()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                [
                    new TestCapabilityDto { TestCode = "TC1" },
                    new TestCapabilityDto { TestCode = "" },
                    new TestCapabilityDto { TestCode = null! }
                ]));

            var result = await _controller.GetTestCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        [Fact]
        public async Task GetTestCodesByWorkGroup_WhenDataIsNull_ReturnsEmptyArray()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(null!));

            var result = await _controller.GetTestCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        #endregion

        #region GetBuyersByTestCode Tests

        [Fact]
        public async Task GetBuyersByTestCode_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetBuyersByTestCode("WG1", "TC1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetBuyersByTestCode_WithTestCode_FiltersByTestCode()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [
                    new TestRequirementDto { TestCode = "TC1", Buyer = "BuyerA" },
                    new TestRequirementDto { TestCode = "TC2", Buyer = "BuyerB" }
                ]));

            var result = await _controller.GetBuyersByTestCode("WG1", "TC1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        [Fact]
        public async Task GetBuyersByTestCode_WithNullTestCode_ReturnsAllBuyers()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [
                    new TestRequirementDto { TestCode = "TC1", Buyer = "BuyerA" },
                    new TestRequirementDto { TestCode = "TC2", Buyer = "BuyerB" }
                ]));

            var result = await _controller.GetBuyersByTestCode("WG1", null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        [Fact]
        public async Task GetBuyersByTestCode_FiltersBlankBuyers()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [
                    new TestRequirementDto { TestCode = "TC1", Buyer = "BuyerA" },
                    new TestRequirementDto { TestCode = "TC1", Buyer = "" },
                    new TestRequirementDto { TestCode = "TC1", Buyer = null! }
                ]));

            var result = await _controller.GetBuyersByTestCode(null, null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        [Fact]
        public async Task GetBuyersByTestCode_WhenDataIsNull_ReturnsEmptyArray()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(null!));

            var result = await _controller.GetBuyersByTestCode(null, null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetBuyersByTestCode_ReturnsDistinctBuyers()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [
                    new TestRequirementDto { TestCode = "TC1", Buyer = "BuyerA" },
                    new TestRequirementDto { TestCode = "TC1", Buyer = "BuyerA" }
                ]));

            var result = await _controller.GetBuyersByTestCode(null, null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        #endregion

        #region GetLiveRecord Tests

        [Fact]
        public async Task GetLiveRecord_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("testCode", "Required");

            var result = await _controller.GetLiveRecord("TC1", "B1", 6, "WG1");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task GetLiveRecord_WhenServiceReturnsNoData_ReturnsNotFound()
        {
            _monthlyOutputService.GetLiveByKeyAsync("TC1", "B1", 6, "WG1")
                .Returns(ApiResponseDto<PactMonthlyOutputDto>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetLiveRecord("TC1", "B1", 6, "WG1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetLiveRecord_WhenServiceSucceeds_ReturnsPartialView()
        {
            var dto = new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1" };
            _monthlyOutputService.GetLiveByKeyAsync("TC1", "B1", 6, "WG1")
                .Returns(ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto));
            _mapper.Map<MonthlyOutputLiveItem>(dto).Returns(new MonthlyOutputLiveItem());

            var result = await _controller.GetLiveRecord("TC1", "B1", 6, "WG1");

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditMonthlyOutputLive", partialView.ViewName);
            Assert.IsType<MonthlyOutputLiveItem>(partialView.Model);
        }

        [Fact]
        public async Task GetLiveRecord_WhenServiceReturnsNullData_ReturnsNotFound()
        {
            _monthlyOutputService.GetLiveByKeyAsync("TC1", "B1", 6, "WG1")
                .Returns(ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(null!));

            var result = await _controller.GetLiveRecord("TC1", "B1", 6, "WG1");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region SaveLiveRecord Tests

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Volume", "Invalid");

            var result = await _controller.SaveLiveRecord(new MonthlyOutputLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.NotNull(success);
            Assert.IsType<bool>(success);
            Assert.False((bool)success);
        }

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_WithDollarDotPrefix_StripsPrefix()
        {
            _controller.ModelState.AddModelError("$.Volume", "Invalid volume");

            var result = await _controller.SaveLiveRecord(new MonthlyOutputLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WithValidationErrors_ReturnsValidationFailureJson()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = "TC1|B1|6|WG1", Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse(
                [
                    new ValidationFieldErrorDto { Field = "Volume", Message = "Invalid" }
                ]));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Validation failed.", message);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateSucceeds_ReturnsSuccessJson()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = "TC1|B1|6|WG1", Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyOutputService.UpdateLiveAsync(dto).Returns(
                ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateFails_ReturnsFailureJson()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = "TC1|B1|6|WG1", Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyOutputService.UpdateLiveAsync(dto).Returns(
                ApiResponseDto<PactMonthlyOutputDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Update failed" }], new ApiMetaDto()));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_WhenUpdateFails_WithNullErrors_ReturnsDefaultErrorMessage()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = "TC1|B1|6|WG1", Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            var failResponse = ApiResponseDto<PactMonthlyOutputDto>.FailureResponse(null, new ApiMetaDto());
            _monthlyOutputService.UpdateLiveAsync(dto).Returns(failResponse);

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveLiveRecord_ParsesCompositeKeyCorrectly()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = "TC1|B1|6|WG1", Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyOutputService.UpdateLiveAsync(dto).Returns(
                ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto));

            await _controller.SaveLiveRecord(model);

            Assert.Equal("TC1", dto.OriginalTestCode);
            Assert.Equal("B1", dto.OriginalBuyer);
            Assert.Equal(6, dto.OriginalMonth);
            Assert.Equal("WG1", dto.OriginalWorkGroup);
        }

        [Fact]
        public async Task SaveLiveRecord_WithNullCompositeKey_HandlesGracefully()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = null!, Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse([]));
            _monthlyOutputService.UpdateLiveAsync(dto).Returns(
                ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto));

            await _controller.SaveLiveRecord(model);

            Assert.Equal(0, dto.OriginalMonth);
        }

        [Fact]
        public async Task SaveLiveRecord_WithValidationResponseNullData_SkipsValidationErrors()
        {
            var model = new MonthlyOutputLiveItem { CompositeKey = "TC1|B1|6|WG1", Volume = 100 };
            var dto = new PactMonthlyOutputDto();
            _mapper.Map<PactMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.ValidateLiveAsync(dto).Returns(
                ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse(null!));
            _monthlyOutputService.UpdateLiveAsync(dto).Returns(
                ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto));

            var result = await _controller.SaveLiveRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        #endregion

        #region GetStagingRecord Tests

        [Fact]
        public async Task GetStagingRecord_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("id", "Required");

            var result = await _controller.GetStagingRecord(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task GetStagingRecord_WhenServiceReturnsNoData_ReturnsNotFound()
        {
            _monthlyOutputService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyOutputDto>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetStagingRecord(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStagingRecord_WhenServiceSucceeds_ReturnsPartialView()
        {
            var dto = new StagingMonthlyOutputDto { Id = 1 };
            _monthlyOutputService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto));
            _mapper.Map<StagingMonthlyOutputItem>(dto).Returns(new StagingMonthlyOutputItem { Id = 1 });

            var result = await _controller.GetStagingRecord(1);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStagingMonthlyOutput", partialView.ViewName);
            Assert.IsType<StagingMonthlyOutputItem>(partialView.Model);
        }

        [Fact]
        public async Task GetStagingRecord_WhenServiceReturnsNullData_ReturnsNotFound()
        {
            _monthlyOutputService.GetStagingByIdAsync(1)
                .Returns(ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(null!));

            var result = await _controller.GetStagingRecord(1);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region AddStagingRecord Tests

        [Fact]
        public async Task AddStagingRecord_ReturnsPartialViewWithEmptyModel()
        {
            var result = await _controller.AddStagingRecord();

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditStagingMonthlyOutput", partialView.ViewName);
            Assert.IsType<StagingMonthlyOutputItem>(partialView.Model);
        }

        #endregion

        #region SaveStagingRecord Tests

        [Fact]
        public async Task SaveStagingRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Volume", "Invalid");

            var result = await _controller.SaveStagingRecord(new StagingMonthlyOutputItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_InvalidModelState_WithDollarDotPrefix_StripsPrefix()
        {
            _controller.ModelState.AddModelError("$.Volume", "Invalid volume");

            var result = await _controller.SaveStagingRecord(new StagingMonthlyOutputItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_CreateNew_WhenSucceeds_ReturnsSuccessJson()
        {
            var model = new StagingMonthlyOutputItem { Id = 0, WorkGroup = "WG1" };
            var dto = new StagingMonthlyOutputDto();
            _mapper.Map<StagingMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.CreateStagingAsync(dto).Returns(
                ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record added successfully.", message);
        }

        [Fact]
        public async Task SaveStagingRecord_Update_WhenSucceeds_ReturnsSuccessJson()
        {
            var model = new StagingMonthlyOutputItem { Id = 5, WorkGroup = "WG1" };
            var dto = new StagingMonthlyOutputDto();
            _mapper.Map<StagingMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.UpdateStagingAsync(5, dto).Returns(
                ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Staging record updated successfully.", message);
            Assert.False(dto.Passed);
        }

        [Fact]
        public async Task SaveStagingRecord_Update_SetsPassedFalseAndFailureComments()
        {
            var model = new StagingMonthlyOutputItem { Id = 5 };
            var dto = new StagingMonthlyOutputDto();
            _mapper.Map<StagingMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.UpdateStagingAsync(5, dto).Returns(
                ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto));

            await _controller.SaveStagingRecord(model);

            Assert.False(dto.Passed);
            Assert.Equal("This record has been edited since being validated. It will need re-validating.", dto.FailureComments);
        }

        [Fact]
        public async Task SaveStagingRecord_WhenServiceFails_ReturnsFailureJson()
        {
            var model = new StagingMonthlyOutputItem { Id = 0, WorkGroup = "WG1" };
            var dto = new StagingMonthlyOutputDto();
            _mapper.Map<StagingMonthlyOutputDto>(model).Returns(dto);

            _monthlyOutputService.CreateStagingAsync(dto).Returns(
                ApiResponseDto<StagingMonthlyOutputDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Failed" }], new ApiMetaDto()));

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task SaveStagingRecord_WhenServiceFails_WithNullErrors_ReturnsDefaultErrorMessage()
        {
            var model = new StagingMonthlyOutputItem { Id = 0 };
            var dto = new StagingMonthlyOutputDto();
            _mapper.Map<StagingMonthlyOutputDto>(model).Returns(dto);

            var failResponse = ApiResponseDto<StagingMonthlyOutputDto>.FailureResponse(null, new ApiMetaDto());
            _monthlyOutputService.CreateStagingAsync(dto).Returns(failResponse);

            var result = await _controller.SaveStagingRecord(model);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region DeleteStagingRecord Tests

        [Fact]
        public async Task DeleteStagingRecord_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("id", "Required");

            var result = await _controller.DeleteStagingRecord(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task DeleteStagingRecord_WhenSucceeds_ReturnsSuccessJson()
        {
            _monthlyOutputService.DeleteStagingAsync(1).Returns(
                ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _controller.DeleteStagingRecord(1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        [Fact]
        public async Task DeleteStagingRecord_WhenFails_ReturnsFailureJson()
        {
            _monthlyOutputService.DeleteStagingAsync(1).Returns(
                ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.DeleteStagingRecord(1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region DeleteAllStagingRecords Tests

        [Fact]
        public async Task DeleteAllStagingRecords_WhenSucceeds_ReturnsSuccessJson()
        {
            _monthlyOutputService.DeleteAllStagingByUserAsync().Returns(
                ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _controller.DeleteAllStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        [Fact]
        public async Task DeleteAllStagingRecords_WhenFails_ReturnsFailureJsonWithMessage()
        {
            _monthlyOutputService.DeleteAllStagingByUserAsync().Returns(
                ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Delete error" }], new ApiMetaDto()));

            var result = await _controller.DeleteAllStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task DeleteAllStagingRecords_WhenFailsWithNoErrors_ReturnsDefaultMessage()
        {
            var response = ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto());
            _monthlyOutputService.DeleteAllStagingByUserAsync().Returns(response);

            var result = await _controller.DeleteAllStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to delete staging records.", message);
        }

        #endregion

        #region DeleteFailedStagingRecords Tests

        [Fact]
        public async Task DeleteFailedStagingRecords_WhenSucceeds_ReturnsSuccessJson()
        {
            _monthlyOutputService.DeleteFailedStagingByUserAsync().Returns(
                ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _controller.DeleteFailedStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
        }

        [Fact]
        public async Task DeleteFailedStagingRecords_WhenFails_ReturnsFailureJson()
        {
            _monthlyOutputService.DeleteFailedStagingByUserAsync().Returns(
                ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Error" }], new ApiMetaDto()));

            var result = await _controller.DeleteFailedStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task DeleteFailedStagingRecords_WhenFailsWithNoErrors_ReturnsDefaultMessage()
        {
            var response = ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto());
            _monthlyOutputService.DeleteFailedStagingByUserAsync().Returns(response);

            var result = await _controller.DeleteFailedStagingRecords();

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Failed to delete failed imported records.", message);
        }

        #endregion

        #region ExportStaging Tests

        [Fact]
        public async Task ExportStaging_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("passed", "Invalid");

            var result = await _controller.ExportStaging(true);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task ExportStaging_WhenServiceFails_ReturnsNotFound()
        {
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyOutputDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.ExportStaging(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportStaging_WhenServiceReturnsNullData_ReturnsNotFound()
        {
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyOutputDto>>.SuccessResponse(null!));

            var result = await _controller.ExportStaging(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportStaging_WhenSucceeds_ReturnsFileResult()
        {
            _monthlyOutputService.GetStagingAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<bool?>())
                .Returns(ApiResponseDto<List<StagingMonthlyOutputDto>>.SuccessResponse([]));

            _mapper.Map<List<StagingMonthlyOutputExportItem>>(Arg.Any<List<StagingMonthlyOutputDto>>())
                .Returns([]);
            _excelExportService.ExportToExcel(Arg.Any<List<StagingMonthlyOutputExportItem>>(), "MonthlyOutput")
                .Returns(new byte[] { 1, 2, 3 });

            var result = await _controller.ExportStaging(true);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.StartsWith("ExportedOP_", fileResult.FileDownloadName);
        }

        #endregion

        #region Import Tests

        [Fact]
        public async Task Import_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("file", "Required");

            var result = await _controller.Import(null!, 1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request data.", badRequest.Value);
        }

        [Fact]
        public async Task Import_NullFile_ReturnsFailureJson()
        {
            var result = await _controller.Import(null!, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Please select an Excel file to import.", message);
        }

        [Fact]
        public async Task Import_EmptyFile_ReturnsFailureJson()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(0);

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task Import_WhenSucceeds_ReturnsSuccessJsonWithCounts()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(1024);

            _monthlyOutputService.ImportMonthlyOutputAsync(file, 1).Returns(
                ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(
                    new MonthlyOutputImportResultDto { ImportedCount = 10, PassedCount = 8, FailedCount = 2, Message = "Done" }));

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var importedCount = json.Value?.GetType().GetProperty("importedCount")?.GetValue(json.Value);
            Assert.Equal(10, importedCount);
        }

        [Fact]
        public async Task Import_WhenFails_ReturnsFailureJsonWithErrorMessage()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(1024);

            _monthlyOutputService.ImportMonthlyOutputAsync(file, 1).Returns(
                ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Import error" }], new ApiMetaDto()));

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Import error", message);
        }

        [Fact]
        public async Task Import_WhenFails_WithNoErrors_ReturnsDefaultMessage()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(1024);

            var response = ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(null, new ApiMetaDto());
            _monthlyOutputService.ImportMonthlyOutputAsync(file, 1).Returns(response);

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Import failed.", message);
        }

        [Fact]
        public async Task Import_WhenSucceedsWithNullData_ReturnsFailureJson()
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(1024);

            _monthlyOutputService.ImportMonthlyOutputAsync(file, 1).Returns(
                ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(null!));

            var result = await _controller.Import(file, 1);

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region Validate Tests

        [Fact]
        public async Task Validate_WhenSucceeds_ReturnsSuccessJsonWithCounts()
        {
            _monthlyOutputService.ValidateStagingAsync().Returns(
                ApiResponseDto<MonthlyOutputValidateResultDto>.SuccessResponse(
                    new MonthlyOutputValidateResultDto { PassedCount = 5, FailedCount = 2, Message = "Validated" }));

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var passedCount = json.Value?.GetType().GetProperty("passedCount")?.GetValue(json.Value);
            Assert.Equal(5, passedCount);
        }

        [Fact]
        public async Task Validate_WhenFails_ReturnsFailureJson()
        {
            _monthlyOutputService.ValidateStagingAsync().Returns(
                ApiResponseDto<MonthlyOutputValidateResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Validation error" }], new ApiMetaDto()));

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Validation error", message);
        }

        [Fact]
        public async Task Validate_WhenFails_WithNoErrors_ReturnsDefaultMessage()
        {
            _monthlyOutputService.ValidateStagingAsync().Returns(
                ApiResponseDto<MonthlyOutputValidateResultDto>.FailureResponse(null, new ApiMetaDto()));

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Validation failed.", message);
        }

        [Fact]
        public async Task Validate_WhenSucceedsWithNullData_ReturnsFailureJson()
        {
            _monthlyOutputService.ValidateStagingAsync().Returns(
                ApiResponseDto<MonthlyOutputValidateResultDto>.SuccessResponse(null!));

            var result = await _controller.Validate();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion

        #region MakeLive Tests

        [Fact]
        public async Task MakeLive_WhenSucceeds_ReturnsSuccessJsonWithCounts()
        {
            _monthlyOutputService.MakeLiveAsync().Returns(
                ApiResponseDto<MonthlyOutputMakeLiveResultDto>.SuccessResponse(
                    new MonthlyOutputMakeLiveResultDto { ProcessedCount = 10, ImportedCount = 8, FailedCount = 2, Message = "Done" }));

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.True((bool)success!);
            var processedCount = json.Value?.GetType().GetProperty("processedCount")?.GetValue(json.Value);
            Assert.Equal(10, processedCount);
        }

        [Fact]
        public async Task MakeLive_WhenFails_ReturnsFailureJson()
        {
            _monthlyOutputService.MakeLiveAsync().Returns(
                ApiResponseDto<MonthlyOutputMakeLiveResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Make live error" }], new ApiMetaDto()));

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Make live error", message);
        }

        [Fact]
        public async Task MakeLive_WhenFails_WithNoErrors_ReturnsDefaultMessage()
        {
            _monthlyOutputService.MakeLiveAsync().Returns(
                ApiResponseDto<MonthlyOutputMakeLiveResultDto>.FailureResponse(null, new ApiMetaDto()));

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var message = json.Value?.GetType().GetProperty("message")?.GetValue(json.Value);
            Assert.Equal("Make live failed.", message);
        }

        [Fact]
        public async Task MakeLive_WhenSucceedsWithNullData_ReturnsFailureJson()
        {
            _monthlyOutputService.MakeLiveAsync().Returns(
                ApiResponseDto<MonthlyOutputMakeLiveResultDto>.SuccessResponse(null!));

            var result = await _controller.MakeLive();

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.False((bool)success!);
        }

        #endregion
    }
}
