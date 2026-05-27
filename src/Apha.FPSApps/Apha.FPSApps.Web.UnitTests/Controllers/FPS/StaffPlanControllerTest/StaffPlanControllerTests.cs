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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.StaffPlanControllerTest
{
    public class StaffPlanControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectStaffPlanService _staffPlanService;
        private readonly StaffPlanController _controller;

        public StaffPlanControllerTests()
        {
            _mapper           = Substitute.For<IMapper>();
            _staffPlanService = Substitute.For<IProjectStaffPlanService>();
            _controller       = new StaffPlanController(_mapper, _staffPlanService);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
        }

        private static List<ProjectStaffPlanViewDto> BuildDtoList() =>
        [
            new() { ParentProject = "AH0032", ProgramNo = "Wildlife", Name = "E_WILDLIFE, General", StaffId = "1625",
                    PlannedHours = 25344, ChargeRate = 53.34m, Cost = 1351848.96m, PayCost = 1001341.44m,
                    WorkGroup = "Wildlife", GradeCode = "E", WgGrade = "E_WILDLIFE", PcGrade = "E_PC", ProfitCentre = "Wildlife" },
            new() { ParentProject = "ED1044", ProgramNo = "SIU",      Name = "C_SVCA, General",     StaffId = "1357",
                    PlannedHours = 12000, ChargeRate = 69.92m, Cost = 839040.00m, PayCost = 624720.00m,
                    WorkGroup = "SVCA",     GradeCode = "C", WgGrade = "C_SVCA",     PcGrade = "C_PC", ProfitCentre = "SIU" }
        ];

        private static ApiResponseDto<List<ProjectStaffPlanViewDto>> SuccessResponse(
            List<ProjectStaffPlanViewDto>? data = null,
            int totalRecords = 2) =>
            ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(
                data ?? BuildDtoList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = totalRecords });

        private static ApiResponseDto<List<ProjectStaffPlanViewDto>> FailureResponse() =>
            ApiResponseDto<List<ProjectStaffPlanViewDto>>.FailureResponse(
                [new ApiErrorDto { Message = "API error", Code = "API_ERROR" }],
                new ApiMetaDto());

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsSuccess_ReturnsViewResult()
        {
            // Arrange
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns(new List<StaffPlanViewItem> { new(), new() });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<StaffPlanViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ServiceReturnsSuccess_ModelContainsGrid()
        {
            // Arrange
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns(new List<StaffPlanViewItem> { new(), new() });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.NotNull(model.Grid);
            Assert.Equal("staffPlanGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_ServiceReturnsSuccess_GridContainsRows()
        {
            // Arrange
            var rows = new List<StaffPlanViewItem> { new(), new() };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns(rows);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.Equal(2, model.Grid.Data.Count);
        }

        [Fact]
        public async Task Index_ServiceReturnsFailure_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_ServiceReturnsNullData_ReturnsViewWithEmptyGrid()
        {
            // Arrange — Success=true but Data=null: the if-branch is skipped, mapper is never called
            var response = new ApiResponseDto<List<ProjectStaffPlanViewDto>>
            {
                Success    = true,
                Data       = null,
                Pagination = null
            };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_ServiceReturnsSuccess_GridHasCorrectConfiguration()
        {
            // Arrange
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
            Assert.True(model.Grid.ShowPagination);
            Assert.Equal("/FPS/StaffPlan/LoadGrid", model.Grid.BindGridUrl);
        }

        [Fact]
        public async Task Index_ServiceReturnsSuccess_PaginationIsPopulated()
        {
            // Arrange
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(totalRecords: 50));
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.Equal(50, model.Grid.Pagination.TotalRecords);
            Assert.Equal(1,  model.Grid.Pagination.PageNumber);
            Assert.Equal(10, model.Grid.Pagination.PageSize);
        }

        [Fact]
        public async Task Index_ServiceReturnsNoPagination_PaginationDefaultsAreUsed()
        {
            // Arrange
            var response = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(BuildDtoList());
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanViewModel>(viewResult.Model);
            Assert.Equal(0, model.Grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task Index_Always_CallsServiceOnce()
        {
            // Arrange
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            await _controller.Index();

            // Assert
            await _staffPlanService.Received(1).GetPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsGridWithData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var rows    = new List<StaffPlanViewItem> { new(), new() };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns(rows);

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<DataGridConfig<StaffPlanViewItem>>(partial.Model);
            Assert.Equal(2, model.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<DataGridConfig<StaffPlanViewItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadGrid_WithSortParameters_PropagatesSortToGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2, PageSize = 5,
                SortBy = "ProgramNo", Descending = true
            };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<DataGridConfig<StaffPlanViewItem>>(partial.Model);
            Assert.Equal("ProgramNo", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_WithFilterJson_PopulatesCurrentFilters()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"ProgramNo":"Wildlife","WorkGroup":"WG1"}"""
            };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<DataGridConfig<StaffPlanViewItem>>(partial.Model);
            Assert.Equal("Wildlife", model.CurrentFilters["ProgramNo"]);
            Assert.Equal("WG1",      model.CurrentFilters["WorkGroup"]);
        }

        [Fact]
        public async Task LoadGrid_WithNullFilter_CurrentFiltersIsEmpty()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = null };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<DataGridConfig<StaffPlanViewItem>>(partial.Model);
            Assert.Empty(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_Always_CallsServiceOnce()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanViewItem>>(Arg.Any<List<ProjectStaffPlanViewDto>>())
                .Returns([]);

            // Act
            await _controller.LoadGrid(request);

            // Assert
            await _staffPlanService.Received(1).GetPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}
