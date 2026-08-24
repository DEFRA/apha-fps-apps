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
            _mapper         = Substitute.For<IMapper>();
            _service        = Substitute.For<ITestorProductService>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _controller     = new TestPriceCheckController(_mapper, _service, _fpsYearContext);
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
                    return new QueryParameters<string>
                    {
                        Page       = f.Page,
                        PageSize   = f.PageSize,
                        SortBy     = f.SortBy,
                        Descending = f.Descending,
                        Filter     = f.Filter
                    };
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

        #region LoadTestPriceCheckGrid – DefraProject Sort

        [Fact]
        public async Task LoadTestPriceCheckGrid_SortByIsDefraProject_MakesTwoServiceCalls()
        {
            // Arrange – request with SortBy = IsDefraProject
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 5,
                SortBy = nameof(TestPriceCheckItem.IsDefraProject),
                Filter = "{}"
            };

            var probeResponse = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(
                [new() { TestCode = "T001", JobCode = "J1" }],
                new PaginationDto { TotalRecords = 3 });

            var allDtos = new List<TestPriceCheckDto>
            {
                new() { TestCode = "T001", JobCode = "J1", IsDefraProject = 0  },
                new() { TestCode = "T002", JobCode = "J2", IsDefraProject = -1 },
                new() { TestCode = "T003", JobCode = "J3", IsDefraProject = -1 }
            };
            var allResponse = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(
                allDtos, new PaginationDto { TotalRecords = 3 });

            _service.GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == 1), "all", null)
                .Returns(probeResponse);
            _service.GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == 3), "all", null)
                .Returns(allResponse);

            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(allDtos).Returns(
                allDtos.Select(d => new TestPriceCheckItem
                    { TestCode = d.TestCode, JobCode = d.JobCode, IsDefraProject = d.IsDefraProject }).ToList());

            // Act
            await _controller.LoadTestPriceCheckGrid(request, "all", null);

            // Assert – probe call (pageSize=1) and full-data call (pageSize=3) each made exactly once
            await _service.Received(1)
                .GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == 1), "all", null);
            await _service.Received(1)
                .GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == 3), "all", null);
        }

        [Fact]
        public async Task LoadTestPriceCheckGrid_SortByIsDefraProject_Ascending_GroupsDefraProjectsFirst()
        {
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 5,
                SortBy = nameof(TestPriceCheckItem.IsDefraProject),
                Descending = false,
                Filter = "{}"
            };

            var allDtos = new List<TestPriceCheckDto>
            {
                new() { TestCode = "T001", JobCode = "J1", IsDefraProject = 0  },
                new() { TestCode = "T002", JobCode = "J2", IsDefraProject = -1 },
                new() { TestCode = "T003", JobCode = "J3", IsDefraProject = 0  },
                new() { TestCode = "T004", JobCode = "J4", IsDefraProject = -1 }
            };
            var allItems = allDtos
                .Select(d => new TestPriceCheckItem
                    { TestCode = d.TestCode, JobCode = d.JobCode, IsDefraProject = d.IsDefraProject })
                .ToList();

            SetupDefraSortService(probeTotal: 4, allDtos: allDtos, pageSize: 5, priceFilter: "all", owner: null);
            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(allDtos).Returns(allItems);

            var result = await _controller.LoadTestPriceCheckGrid(request, "all", null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestPriceCheckItem>>(partial.Model);
            // Ascending: -1 (Defra) < 0 (non-Defra), so Defra items are first
            Assert.All(grid.Data.Take(2), item => Assert.Equal(-1, item.IsDefraProject));
            Assert.All(grid.Data.Skip(2), item => Assert.Equal(0,  item.IsDefraProject));
        }

        [Fact]
        public async Task LoadTestPriceCheckGrid_SortByIsDefraProject_Descending_GroupsNonDefraProjectsFirst()
        {
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 5,
                SortBy = nameof(TestPriceCheckItem.IsDefraProject),
                Descending = true,
                Filter = "{}"
            };

            var allDtos = new List<TestPriceCheckDto>
            {
                new() { TestCode = "T001", JobCode = "J1", IsDefraProject = -1 },
                new() { TestCode = "T002", JobCode = "J2", IsDefraProject = 0  },
                new() { TestCode = "T003", JobCode = "J3", IsDefraProject = -1 },
                new() { TestCode = "T004", JobCode = "J4", IsDefraProject = 0  }
            };
            var allItems = allDtos
                .Select(d => new TestPriceCheckItem
                    { TestCode = d.TestCode, JobCode = d.JobCode, IsDefraProject = d.IsDefraProject })
                .ToList();

            SetupDefraSortService(probeTotal: 4, allDtos: allDtos, pageSize: 5, priceFilter: "all", owner: null);
            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(allDtos).Returns(allItems);

            var result = await _controller.LoadTestPriceCheckGrid(request, "all", null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestPriceCheckItem>>(partial.Model);
            // Descending: 0 (non-Defra) > -1 (Defra), so non-Defra items are first
            Assert.All(grid.Data.Take(2), item => Assert.Equal(0,  item.IsDefraProject));
            Assert.All(grid.Data.Skip(2), item => Assert.Equal(-1, item.IsDefraProject));
        }

        [Fact]
        public async Task LoadTestPriceCheckGrid_SortByIsDefraProject_CorrectPageSliceReturnedForPage2()
        {
            // 4 records total, pageSize=2, page=2 → slice [2..3] of the sorted set
            var request = new PaginationFilter<string>
            {
                Page = 2, PageSize = 2,
                SortBy = nameof(TestPriceCheckItem.IsDefraProject),
                Descending = false,
                Filter = "{}"
            };

            // Sorted ascending: T002(-1), T004(-1), T001(0), T003(0)
            // Page 2 (skip 2, take 2) → T001, T003
            var allDtos = new List<TestPriceCheckDto>
            {
                new() { TestCode = "T001", JobCode = "J1", IsDefraProject = 0  },
                new() { TestCode = "T002", JobCode = "J2", IsDefraProject = -1 },
                new() { TestCode = "T003", JobCode = "J3", IsDefraProject = 0  },
                new() { TestCode = "T004", JobCode = "J4", IsDefraProject = -1 }
            };
            var allItems = allDtos
                .Select(d => new TestPriceCheckItem
                    { TestCode = d.TestCode, JobCode = d.JobCode, IsDefraProject = d.IsDefraProject })
                .ToList();

            SetupDefraSortService(probeTotal: 4, allDtos: allDtos, pageSize: 4, priceFilter: "all", owner: null);
            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(allDtos).Returns(allItems);

            var result = await _controller.LoadTestPriceCheckGrid(request, "all", null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestPriceCheckItem>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
            // Page 2 of ascending sort should be the two non-Defra (0) items
            Assert.All(grid.Data, item => Assert.Equal(0, item.IsDefraProject));
        }

        [Fact]
        public async Task LoadTestPriceCheckGrid_SortByIsDefraProject_TotalRecordsSetFromProbeResponse()
        {
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 5,
                SortBy = nameof(TestPriceCheckItem.IsDefraProject),
                Filter = "{}"
            };

            var allDtos = new List<TestPriceCheckDto>
            {
                new() { TestCode = "T001", JobCode = "J1", IsDefraProject = -1 },
                new() { TestCode = "T002", JobCode = "J2", IsDefraProject = 0  }
            };
            var allItems = allDtos
                .Select(d => new TestPriceCheckItem
                    { TestCode = d.TestCode, JobCode = d.JobCode, IsDefraProject = d.IsDefraProject })
                .ToList();

            SetupDefraSortService(probeTotal: 42, allDtos: allDtos, pageSize: 5, priceFilter: "all", owner: null);
            SetupQueryParamMapper();
            _mapper.Map<List<TestPriceCheckItem>>(allDtos).Returns(allItems);

            var result = await _controller.LoadTestPriceCheckGrid(request, "all", null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestPriceCheckItem>>(partial.Model);
            Assert.Equal(42, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadTestPriceCheckGrid_SortByIsDefraProject_ZeroTotalRecords_ReturnsEmptyGridWithoutSecondCall()
        {
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 5,
                SortBy = nameof(TestPriceCheckItem.IsDefraProject),
                Filter = "{}"
            };

            var probeResponse = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(
                [], new PaginationDto { TotalRecords = 0 });

            _service.GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == 1), "all", null)
                .Returns(probeResponse);

            SetupQueryParamMapper();

            var result = await _controller.LoadTestPriceCheckGrid(request, "all", null);

            // Full-data call (pageSize != 1) must never be made when TotalRecords = 0
            await _service.DidNotReceive()
                .GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize != 1), "all", null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestPriceCheckItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        /// <summary>
        /// Helper: wires up the probe (pageSize=1) and full-data (pageSize=probeTotal) service calls
        /// for the IsDefraProject two-call sort path.
        /// </summary>
        private void SetupDefraSortService(
            int probeTotal,
            List<TestPriceCheckDto> allDtos,
            int pageSize,
            string priceFilter,
            string? owner)
        {
            var probeResponse = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(
                allDtos.Take(1).ToList(),
                new PaginationDto { TotalRecords = probeTotal });

            var allResponse = ApiResponseDto<List<TestPriceCheckDto>>.SuccessResponse(
                allDtos,
                new PaginationDto { TotalRecords = probeTotal });

            _service.GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == 1), priceFilter, owner)
                .Returns(probeResponse);

            _service.GetTestPriceCheckPagedAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == probeTotal), priceFilter, owner)
                .Returns(allResponse);
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
