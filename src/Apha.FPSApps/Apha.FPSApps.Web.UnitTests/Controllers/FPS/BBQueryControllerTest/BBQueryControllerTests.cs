using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BBQueryControllerTest
{
    public class BBQueryControllerTests
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly BBQueryController _controller;

        public BBQueryControllerTests()
        {
            _workGroupService = Substitute.For<IWorkGroupService>();
            _budgetBidsService = Substitute.For<IBudgetBidsService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _controller = new BBQueryController(_workGroupService, _budgetBidsService, _profitCentreService)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private static ApiResponseDto<T> Fail<T>() =>
            ApiResponseDto<T>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());

        [Fact]
        public async Task Index_ReturnsView_WithEmptyGrid_AndFiltersBlankProfitCentres()
        {
            _controller.HttpContext.Items["SelectedFPSYear"] = "2025";
            var profitCentres = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "One" },
                new() { ProfitCentreId = "  ", ProfitCentreName = "Blank" }
            };
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BBQueryViewModel>(view.Model);
            Assert.Equal(2025, model.FpsYear);
            Assert.Null(model.SelectedProfitCentre);
            Assert.Single(model.ProfitCentreOptions);
            Assert.Equal("PC1", model.ProfitCentreOptions[0].Value);
            // Empty grid: only the two fixed columns and no rows.
            Assert.Equal(2, model.Grid.Columns.Count);
            Assert.Empty(model.Grid.Data);
            Assert.Equal("bbQueryGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_UsesCurrentYear_WhenSelectedFpsYearItemMissing()
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>()));

            var result = await _controller.Index();

            var model = Assert.IsType<BBQueryViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_UsesCurrentYear_WhenSelectedFpsYearItemNotParsable()
        {
            _controller.HttpContext.Items["SelectedFPSYear"] = "not-a-year";
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>()));

            var result = await _controller.Index();

            var model = Assert.IsType<BBQueryViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_ProfitCentreOptions_Empty_WhenServiceFails()
        {
            _profitCentreService.GetProfitCentresAsync().Returns(Fail<List<ProfitCentreDto>>());

            var result = await _controller.Index();

            var model = Assert.IsType<BBQueryViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.ProfitCentreOptions);
        }

        [Fact]
        public async Task LoadGrid_NullProfitCentre_ReturnsEmptyGridPartial()
        {
            var result = await _controller.LoadGrid(null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<BBQueryCrosstabRow>>(partial.Model);
            Assert.Equal(2, grid.Columns.Count);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WithProfitCentre_BuildsCrosstab_WithDynamicColumnsAndSummaries()
        {
            var workgroups = new List<WorkGroupViewDto>
            {
                new() { WorkGroupName = "WG2", ProfitCentre = "PC1" },
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
            };
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(workgroups));

            _budgetBidsService.GetBidViewAsync("WG1").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG1", GenBid = 10m },
                    new() { Account = "A2", WorkGroupName = "WG1", GenBid = 5m }
                }));
            _budgetBidsService.GetBidViewAsync("WG2").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG2", GenBid = 20m }
                }));

            _budgetBidsService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto>
                {
                    new() { AccShortName = "A2" },
                    new() { AccShortName = "A1" }
                }));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<BBQueryCrosstabRow>>(Assert.IsType<PartialViewResult>(result).Model);

            // Columns: AccShortName, RowSummary, then workgroups ordered ascending (WG1, WG2).
            Assert.Collection(grid.Columns.Select(c => c.PropertyName),
                p => Assert.Equal("AccShortName", p),
                p => Assert.Equal("RowSummary", p),
                p => Assert.Equal("WG1", p),
                p => Assert.Equal("WG2", p));

            // Rows ordered by account (A1, A2).
            Assert.Equal(2, grid.Data.Count);
            var a1 = grid.Data[0];
            Assert.Equal("A1", a1.AccShortName);
            Assert.Equal(10m, a1.Values["WG1"]);
            Assert.Equal(20m, a1.Values["WG2"]);
            Assert.Equal(30m, a1.RowSummary);

            var a2 = grid.Data[1];
            Assert.Equal("A2", a2.AccShortName);
            Assert.Equal(5m, a2.Values["WG1"]);
            Assert.Equal(0m, a2.Values["WG2"]);
            Assert.Equal(5m, a2.RowSummary);
        }

        [Fact]
        public async Task LoadGrid_FallsBackToBidAccounts_WhenNoCategoriesReturned()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
                }));
            _budgetBidsService.GetBidViewAsync("WG1").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG1", GenBid = 10m }
                }));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<BBQueryCrosstabRow>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Single(grid.Data);
            Assert.Equal("A1", grid.Data[0].AccShortName);
            Assert.Equal(10m, grid.Data[0].RowSummary);
        }

        [Fact]
        public async Task LoadGrid_WorkGroupServiceFails_ProducesNoRows()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(Fail<List<WorkGroupViewDto>>());
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<BBQueryCrosstabRow>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Columns.Count);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_SkipsWorkgroup_WhenBidServiceFails()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                    new() { WorkGroupName = "WG2", ProfitCentre = "PC1" }
                }));
            _budgetBidsService.GetBidViewAsync("WG1").Returns(Fail<List<BidViewDto>>());
            _budgetBidsService.GetBidViewAsync("WG2").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG2", GenBid = 15m }
                }));
            _budgetBidsService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto> { new() { AccShortName = "A1" } }));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<BBQueryCrosstabRow>>(Assert.IsType<PartialViewResult>(result).Model);
            var a1 = Assert.Single(grid.Data);
            Assert.Equal(0m, a1.Values["WG1"]);
            Assert.Equal(15m, a1.Values["WG2"]);
            Assert.Equal(15m, a1.RowSummary);
        }
    }
}
