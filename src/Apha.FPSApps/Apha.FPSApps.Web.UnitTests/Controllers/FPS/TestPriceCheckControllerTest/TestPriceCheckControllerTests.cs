using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestPriceCheckControllerTest
{
    public class TestPriceCheckControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestorProductService _service;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly TestPriceCheckController _controller;

        public TestPriceCheckControllerTests()
        {
            _mapper        = Substitute.For<IMapper>();
            _service       = Substitute.For<ITestorProductService>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _controller    = new TestPriceCheckController(_mapper, _service, _fpsYearContext);
        }

        private static T? FromJson<T>(JsonResult result)
        {
            var json = JsonSerializer.Serialize(result.Value);
            return JsonSerializer.Deserialize<T>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void SetupQueryParamMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(c =>
                {
                    var f = c.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string> { Page = f.Page, PageSize = f.PageSize, Filter = f.Filter };
                });
        }

        private void SetupItemMapper(List<TestPriceCheckDto> dtos, List<TestPriceCheckItem> items)
        {
            _mapper.Map<List<TestPriceCheckItem>>(dtos).Returns(items);
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            var dtos     = new List<TestPriceCheckDto> { new() { TestCode = "T001", JobCode = "JOB001" } };
            var items    = new List<TestPriceCheckItem> { new() { TestCode = "T001", JobCode = "JOB001" } };
            var owners   = new List<string> { "AB", "CD" };
            var pagedRes = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(dtos, new PaginationDto());
            var ownersRes = ApiResponseDto<List<string>>.SuccessResponse(owners);

            _service.GetTestPriceCheckPagedAsync(Arg.Any<QueryParameters<string>>(), "all", null).Returns(pagedRes);
            _service.GetOwnersAsync().Returns(ownersRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestPriceCheckViewModel>(view.Model);
            Assert.NotNull(model.PriceCheckGrid);
            Assert.Equal("testPriceCheckGrid", model.PriceCheckGrid.GridId);
            Assert.Equal("all", model.SelectedPriceFilter);
        }

        [Fact]
        public async Task Index_PopulatesOwnersList()
        {
            var owners    = new List<string> { "AB", "CD" };
            var ownersRes = ApiResponseDto<List<string>>.SuccessResponse(owners);
            var pagedRes  = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse([], new PaginationDto());

            _service.GetTestPriceCheckPagedAsync(Arg.Any<QueryParameters<string>>(), "all", null).Returns(pagedRes);
            _service.GetOwnersAsync().Returns(ownersRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(Arg.Any<List<TestPriceCheckDto>>()).Returns([]);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestPriceCheckViewModel>(view.Model);
            Assert.Equal(owners, model.Owners);
        }

        #endregion

        #region LoadTestPriceCheckGrid

        [Fact]
        public async Task LoadTestPriceCheckGrid_ValidRequest_ReturnsPartialView()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtos     = new List<TestPriceCheckDto> { new() { TestCode = "T001", JobCode = "JOB001" } };
            var items    = new List<TestPriceCheckItem> { new() { TestCode = "T001", JobCode = "JOB001" } };
            var pagedRes = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetTestPriceCheckPagedAsync(Arg.Any<QueryParameters<string>>(), "all", null).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.LoadTestPriceCheckGrid(request, "all", null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadTestPriceCheckGrid_WithPriceFilterAndOwner_PassesThemToService()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagedRes = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse([], new PaginationDto());

            _service.GetTestPriceCheckPagedAsync(Arg.Any<QueryParameters<string>>(), "zero", "AB").Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(Arg.Any<List<TestPriceCheckDto>>()).Returns([]);

            await _controller.LoadTestPriceCheckGrid(request, "zero", "AB");

            await _service.Received(1)
                .GetTestPriceCheckPagedAsync(Arg.Any<QueryParameters<string>>(), "zero", "AB");
        }

        #endregion

        #region Edit GET

        [Fact]
        public async Task Edit_Get_ExistingKey_ReturnsPartialViewWithModel()
        {
            var dto = new TestPriceCheckDto
            {
                TestCode = "T001", JobCode = "JOB001", IsDefraProject = 0,
                TestPrice = 50m, NormalPrice = 50m
            };
            var item = new TestPriceCheckItem
            {
                TestCode = "T001", JobCode = "JOB001", IsDefraProject = 0,
                TestPrice = 50m, NormalPrice = 50m
            };
            var response = ApiResponseDto<TestPriceCheckDto>.SuccessResponse(dto);

            _service.GetTestPriceCheckByKeyAsync("T001", "JOB001").Returns(response);
            _mapper.Map<TestPriceCheckItem>(dto).Returns(item);

            var result = await _controller.Edit("T001", "JOB001");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditTestPriceCheck", partial.ViewName);
            var model = Assert.IsType<TestPriceCheckItem>(partial.Model);
            Assert.Equal("T001",   model.TestCode);
            Assert.Equal("JOB001", model.JobCode);
        }

        [Fact]
        public async Task Edit_Get_PopulatesIsDefraProjectList()
        {
            var dto  = new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001", IsDefraProject = -1 };
            var item = new TestPriceCheckItem { TestCode = "T001", JobCode = "JOB001", IsDefraProject = -1 };
            var response = ApiResponseDto<TestPriceCheckDto>.SuccessResponse(dto);

            _service.GetTestPriceCheckByKeyAsync("T001", "JOB001").Returns(response);
            _mapper.Map<TestPriceCheckItem>(dto).Returns(item);

            var result = await _controller.Edit("T001", "JOB001");

            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<TestPriceCheckItem>(partial.Model);
            Assert.Equal(2, model.IsDefraProjectList.Count);
            Assert.True(model.IsDefraProjectList.First(x => x.Value == "-1").Selected);
            Assert.False(model.IsDefraProjectList.First(x => x.Value == "0").Selected);
        }

        [Fact]
        public async Task Edit_Get_NotFoundResponse_ReturnsNotFound()
        {
            var errors   = new List<ApiErrorDto> { new() { Message = "Not found" } };
            var response = ApiResponseDto<TestPriceCheckDto>.FailureResponse(errors, new ApiMetaDto());
            _service.GetTestPriceCheckByKeyAsync("MISSING", "MISSING").Returns(response);

            var result = await _controller.Edit("MISSING", "MISSING");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Edit POST

        [Fact]
        public async Task Edit_Post_Success_ReturnsJsonWithSuccessTrue()
        {
            var model = new TestPriceCheckItem
            {
                TestCode = "T001", JobCode = "JOB001",
                IsDefraProject = -1, TestPrice = 75m, DefraUnitPrice = 120m
            };
            var dto      = new TestPriceCheckDto { IsDefraProject = -1, TestPrice = 75m, DefraUnitPrice = 120m };
            var response = ApiResponseDto<bool>.SuccessResponse(true);

            _mapper.Map<TestPriceCheckDto>(model).Returns(dto);
            _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            var result = await _controller.Edit(model);

            var json    = Assert.IsType<JsonResult>(result);
            var payload = FromJson<JsonPayload>(json);
            Assert.True(payload!.Success);
            Assert.Equal("Test price updated successfully.", payload.Message);
        }

        [Fact]
        public async Task Edit_Post_ServiceFails_ReturnsJsonWithSuccessFalse()
        {
            var model    = new TestPriceCheckItem { TestCode = "T001", JobCode = "JOB001" };
            var dto      = new TestPriceCheckDto();
            var errors   = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERR" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestPriceCheckDto>(model).Returns(dto);
            _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            var result = await _controller.Edit(model);

            var json    = Assert.IsType<JsonResult>(result);
            var payload = FromJson<JsonPayload>(json);
            Assert.False(payload!.Success);
            Assert.Equal("Update failed", payload.Message);
        }

        [Fact]
        public async Task Edit_Post_CallsServiceWithCorrectKeys()
        {
            var model    = new TestPriceCheckItem { TestCode = "T001", JobCode = "JOB001" };
            var dto      = new TestPriceCheckDto();
            var response = ApiResponseDto<bool>.SuccessResponse(true);

            _mapper.Map<TestPriceCheckDto>(model).Returns(dto);
            _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            await _controller.Edit(model);

            await _service.Received(1).UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto);
        }

        [Fact]
        public async Task Edit_Post_NoErrorsInFailureResponse_ReturnsFallbackMessage()
        {
            var model    = new TestPriceCheckItem { TestCode = "T001", JobCode = "JOB001" };
            var dto      = new TestPriceCheckDto();
            var response = ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto());

            _mapper.Map<TestPriceCheckDto>(model).Returns(dto);
            _service.UpdateTestPriceCheckByKeyAsync("T001", "JOB001", dto).Returns(response);

            var result = await _controller.Edit(model);

            var json    = Assert.IsType<JsonResult>(result);
            var payload = FromJson<JsonPayload>(json);
            Assert.False(payload!.Success);
            Assert.Equal("Failed to update test price.", payload.Message);
        }

        private record JsonPayload(bool Success, string? Message);

        #endregion
    }
}
