using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.BosworthInterfaceControllerTest
{
    public class BosworthInterfaceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IBosworthInterfaceService _bosworthInterfaceService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IProjectService _projectService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IExcelExportService _excelExportService;
        private readonly BosworthInterfaceController _controller;

        public BosworthInterfaceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _bosworthInterfaceService = Substitute.For<IBosworthInterfaceService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _projectService = Substitute.For<IProjectService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _excelExportService = Substitute.For<IExcelExportService>();
            _controller = new BosworthInterfaceController(
                _mapper,
                _bosworthInterfaceService,
                _workGroupService,
                _projectService,
                _profitCentreService,
                _excelExportService);
        }

        private void SetupDropdownsSuccess()
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    [new ProjectDto { ParentProject = "P1", Manager = "M1" }]));
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(
                    new List<ProfitCentreDto> { new() { ProfitCentreId = "PC1", Division = "D1", ProfitCentreName = "PCName1" } }));
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                    [new WorkGroupDto { WorkGroupName = "WG1", ProfitCentre = "PC1" }]));

            _mapper.Map<List<Project>>(Arg.Any<List<ProjectDto>>())
                .Returns([new Project { ParentProject = "P1", Manager = "M1" }]);
            _mapper.Map<List<ProfitCentre>>(Arg.Any<IEnumerable<ProfitCentreDto>>())
                .Returns([new ProfitCentre { Division = "D1", ProfitCentreName = "PCName1" }]);
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns([new WorkGroup { WorkGroupName = "WG1", ProfitCentre = "PC1" }]);
        }

        private void SetupDropdownsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto()));
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto()));
        }

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDropdownsSuccess();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_WhenDropdownsLoadSuccessfully_PopulatesViewModelOptions()
        {
            // Arrange
            SetupDropdownsSuccess();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
            Assert.Single(model.ProjectOptions);
            Assert.Single(model.ProfitCentreOptions);
            Assert.Single(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WhenDropdownServicesFail_ReturnsViewWithEmptyDropdowns()
        {
            // Arrange
            SetupDropdownsFailure();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectOptions);
            Assert.Empty(model.ProfitCentreOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WhenDropdownsReturnNullData_ReturnsViewWithEmptyDropdowns()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(new ApiResponseDto<List<ProjectDto>> { Success = true, Data = null });
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(new ApiResponseDto<IEnumerable<ProfitCentreDto>> { Success = true, Data = null });
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(new ApiResponseDto<List<WorkGroupDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectOptions);
            Assert.Empty(model.ProfitCentreOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_ProjectsOrderedByParentProject()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    [new ProjectDto { ParentProject = "B1" }, new ProjectDto { ParentProject = "A1" }]));
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(
                    new List<ProfitCentreDto>()));
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));

            _mapper.Map<List<Project>>(Arg.Any<List<ProjectDto>>())
                .Returns([new Project { ParentProject = "B1" }, new Project { ParentProject = "A1" }]);
            _mapper.Map<List<ProfitCentre>>(Arg.Any<IEnumerable<ProfitCentreDto>>())
                .Returns([]);
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
            Assert.Equal("A1", model.ProjectOptions[0].ParentProject);
            Assert.Equal("B1", model.ProjectOptions[1].ParentProject);
        }

        [Fact]
        public async Task Index_ProfitCentresOrderedByDivision()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(
                    new List<ProfitCentreDto>
                    {
                        new() { Division = "Z", ProfitCentreName = "PCZ" },
                        new() { Division = "A", ProfitCentreName = "PCA" }
                    }));
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));

            _mapper.Map<List<Project>>(Arg.Any<List<ProjectDto>>())
                .Returns([]);
            _mapper.Map<List<ProfitCentre>>(Arg.Any<IEnumerable<ProfitCentreDto>>())
                .Returns([new ProfitCentre { Division = "Z", ProfitCentreName = "PCZ" }, new ProfitCentre { Division = "A", ProfitCentreName = "PCA" }]);
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns([]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
            Assert.Equal("A", model.ProfitCentreOptions[0].Division);
            Assert.Equal("Z", model.ProfitCentreOptions[1].Division);
        }

        [Fact]
        public async Task Index_WorkGroupsOrderedByWorkGroupName()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
            _profitCentreService.GetAllProfitCentresAsync()
                .Returns(ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(
                    new List<ProfitCentreDto>()));
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                    [new WorkGroupDto { WorkGroupName = "WG-Z" }, new WorkGroupDto { WorkGroupName = "WG-A" }]));

            _mapper.Map<List<Project>>(Arg.Any<List<ProjectDto>>())
                .Returns([]);
            _mapper.Map<List<ProfitCentre>>(Arg.Any<IEnumerable<ProfitCentreDto>>())
                .Returns([]);
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns([new WorkGroup { WorkGroupName = "WG-Z" }, new WorkGroup { WorkGroupName = "WG-A" }]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BosworthInterfaceViewModel>(viewResult.Model);
            Assert.Equal("WG-A", model.WorkGroupOptions[0].WorkGroupName);
            Assert.Equal("WG-Z", model.WorkGroupOptions[1].WorkGroupName);
        }

        #endregion

        #region GenerateTimePurchaseProjectReport

        [Fact]
        public async Task GenerateTimePurchaseProjectReport_ReturnsFileContentResult()
        {
            // Arrange
            var project = "P1";
            var fileBytes = new byte[] { 1, 2, 3, 4 };
            _bosworthInterfaceService.GetTimePurchaseProjectAsync(project)
                .Returns(ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse(
                    [new TimePurchaseProjectDto { Project = "P1" }]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimePurchaseProjectDto>>(), "TimePurchaseProject")
                .Returns(fileBytes);

            // Act
            var result = await _controller.GenerateTimePurchaseProjectReport(project);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal($"TimePurchaseProject_{project}.xlsx", fileResult.FileDownloadName);
            Assert.Equal(fileBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GenerateTimePurchaseProjectReport_CallsServiceWithCorrectProject()
        {
            // Arrange
            var project = "P1";
            _bosworthInterfaceService.GetTimePurchaseProjectAsync(project)
                .Returns(ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse([]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimePurchaseProjectDto>>(), "TimePurchaseProject")
                .Returns(new byte[] { 1 });

            // Act
            await _controller.GenerateTimePurchaseProjectReport(project);

            // Assert
            await _bosworthInterfaceService.Received(1).GetTimePurchaseProjectAsync(project);
        }

        [Fact]
        public async Task GenerateTimePurchaseProjectReport_WhenDataIsNull_PassesEmptyListToExcel()
        {
            // Arrange
            var project = "P1";
            var response = new ApiResponseDto<List<TimePurchaseProjectDto>> { Success = false, Data = null };
            _bosworthInterfaceService.GetTimePurchaseProjectAsync(project).Returns(response);
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimePurchaseProjectDto>>(), "TimePurchaseProject")
                .Returns(new byte[] { 1 });

            // Act
            var result = await _controller.GenerateTimePurchaseProjectReport(project);

            // Assert
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1).ExportToExcel(
                Arg.Is<IEnumerable<TimePurchaseProjectDto>>(d => !d.Any()), "TimePurchaseProject");
        }

        #endregion

        #region GenerateTimeSaleProfitCentreReport

        [Fact]
        public async Task GenerateTimeSaleProfitCentreReport_ReturnsFileContentResult()
        {
            // Arrange
            var profitCentre = "PC1";
            var fileBytes = new byte[] { 5, 6, 7, 8 };
            _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(profitCentre)
                .Returns(ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse(
                    [new TimeSaleProfitCentreDto { ProfitCentre = "PC1" }]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimeSaleProfitCentreDto>>(), "TimeSaleProfitCentre")
                .Returns(fileBytes);

            // Act
            var result = await _controller.GenerateTimeSaleProfitCentreReport(profitCentre);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal($"TimeSaleProfitCentre_{profitCentre}.xlsx", fileResult.FileDownloadName);
            Assert.Equal(fileBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GenerateTimeSaleProfitCentreReport_CallsServiceWithCorrectProfitCentre()
        {
            // Arrange
            var profitCentre = "PC1";
            _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(profitCentre)
                .Returns(ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse([]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimeSaleProfitCentreDto>>(), "TimeSaleProfitCentre")
                .Returns(new byte[] { 1 });

            // Act
            await _controller.GenerateTimeSaleProfitCentreReport(profitCentre);

            // Assert
            await _bosworthInterfaceService.Received(1).GetTimeSaleProfitCentreAsync(profitCentre);
        }

        [Fact]
        public async Task GenerateTimeSaleProfitCentreReport_WhenDataIsNull_PassesEmptyListToExcel()
        {
            // Arrange
            var profitCentre = "PC1";
            var response = new ApiResponseDto<List<TimeSaleProfitCentreDto>> { Success = false, Data = null };
            _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(profitCentre).Returns(response);
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimeSaleProfitCentreDto>>(), "TimeSaleProfitCentre")
                .Returns(new byte[] { 1 });

            // Act
            var result = await _controller.GenerateTimeSaleProfitCentreReport(profitCentre);

            // Assert
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1).ExportToExcel(
                Arg.Is<IEnumerable<TimeSaleProfitCentreDto>>(d => !d.Any()), "TimeSaleProfitCentre");
        }

        #endregion

        #region GenerateTimeSaleWorkgroupReport

        [Fact]
        public async Task GenerateTimeSaleWorkgroupReport_ReturnsFileContentResult()
        {
            // Arrange
            var workGroup = "WG1";
            var fileBytes = new byte[] { 9, 10, 11 };
            _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(workGroup)
                .Returns(ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse(
                    [new TimeSaleProfitCentreDto { WorkGroup = "WG1" }]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimeSaleProfitCentreDto>>(), "TimeSaleWorkgroup")
                .Returns(fileBytes);

            // Act
            var result = await _controller.GenerateTimeSaleWorkgroupReport(workGroup);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal($"TimeSaleWorkgroup_{workGroup}.xlsx", fileResult.FileDownloadName);
            Assert.Equal(fileBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GenerateTimeSaleWorkgroupReport_CallsGetTimeSaleProfitCentreAsyncWithWorkGroup()
        {
            // Arrange
            var workGroup = "WG1";
            _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(workGroup)
                .Returns(ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse([]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimeSaleProfitCentreDto>>(), "TimeSaleWorkgroup")
                .Returns(new byte[] { 1 });

            // Act
            await _controller.GenerateTimeSaleWorkgroupReport(workGroup);

            // Assert
            await _bosworthInterfaceService.Received(1).GetTimeSaleProfitCentreAsync(workGroup);
        }

        [Fact]
        public async Task GenerateTimeSaleWorkgroupReport_WhenDataIsNull_PassesEmptyListToExcel()
        {
            // Arrange
            var workGroup = "WG1";
            var response = new ApiResponseDto<List<TimeSaleProfitCentreDto>> { Success = false, Data = null };
            _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(workGroup).Returns(response);
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TimeSaleProfitCentreDto>>(), "TimeSaleWorkgroup")
                .Returns(new byte[] { 1 });

            // Act
            var result = await _controller.GenerateTimeSaleWorkgroupReport(workGroup);

            // Assert
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1).ExportToExcel(
                Arg.Is<IEnumerable<TimeSaleProfitCentreDto>>(d => !d.Any()), "TimeSaleWorkgroup");
        }

        #endregion

        #region GenerateTestSaleSellingWorkgroupReport

        [Fact]
        public async Task GenerateTestSaleSellingWorkgroupReport_ReturnsFileContentResult()
        {
            // Arrange
            var workGroup = "WG1";
            var fileBytes = new byte[] { 12, 13, 14 };
            _bosworthInterfaceService.GetTestSaleSellingWorkgroupAsync(workGroup)
                .Returns(ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse(
                    [new TestSaleSellingWorkgroupDto { SellerWG = "WG1" }]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TestSaleSellingWorkgroupDto>>(), "TestSaleSellingWorkgroup")
                .Returns(fileBytes);

            // Act
            var result = await _controller.GenerateTestSaleSellingWorkgroupReport(workGroup);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal($"TestSaleSellingWorkgroup_{workGroup}.xlsx", fileResult.FileDownloadName);
            Assert.Equal(fileBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GenerateTestSaleSellingWorkgroupReport_CallsServiceWithCorrectWorkGroup()
        {
            // Arrange
            var workGroup = "WG1";
            _bosworthInterfaceService.GetTestSaleSellingWorkgroupAsync(workGroup)
                .Returns(ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse([]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TestSaleSellingWorkgroupDto>>(), "TestSaleSellingWorkgroup")
                .Returns(new byte[] { 1 });

            // Act
            await _controller.GenerateTestSaleSellingWorkgroupReport(workGroup);

            // Assert
            await _bosworthInterfaceService.Received(1).GetTestSaleSellingWorkgroupAsync(workGroup);
        }

        [Fact]
        public async Task GenerateTestSaleSellingWorkgroupReport_WhenDataIsNull_PassesEmptyListToExcel()
        {
            // Arrange
            var workGroup = "WG1";
            var response = new ApiResponseDto<List<TestSaleSellingWorkgroupDto>> { Success = false, Data = null };
            _bosworthInterfaceService.GetTestSaleSellingWorkgroupAsync(workGroup).Returns(response);
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TestSaleSellingWorkgroupDto>>(), "TestSaleSellingWorkgroup")
                .Returns(new byte[] { 1 });

            // Act
            var result = await _controller.GenerateTestSaleSellingWorkgroupReport(workGroup);

            // Assert
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1).ExportToExcel(
                Arg.Is<IEnumerable<TestSaleSellingWorkgroupDto>>(d => !d.Any()), "TestSaleSellingWorkgroup");
        }

        #endregion

        #region GenerateTestSaleBuyingProjectReport

        [Fact]
        public async Task GenerateTestSaleBuyingProjectReport_ReturnsFileContentResult()
        {
            // Arrange
            var parentProject = "PP1";
            var fileBytes = new byte[] { 15, 16, 17 };
            _bosworthInterfaceService.GetTestSaleBuyingProjectAsync(parentProject)
                .Returns(ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse(
                    [new TestSaleBuyingProjectDto { Buyer = "B1" }]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TestSaleBuyingProjectDto>>(), "TestSaleBuyingProject")
                .Returns(fileBytes);

            // Act
            var result = await _controller.GenerateTestSaleBuyingProjectReport(parentProject);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal($"TestSaleBuyingProject_{parentProject}.xlsx", fileResult.FileDownloadName);
            Assert.Equal(fileBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task GenerateTestSaleBuyingProjectReport_CallsServiceWithCorrectParentProject()
        {
            // Arrange
            var parentProject = "PP1";
            _bosworthInterfaceService.GetTestSaleBuyingProjectAsync(parentProject)
                .Returns(ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse([]));
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TestSaleBuyingProjectDto>>(), "TestSaleBuyingProject")
                .Returns(new byte[] { 1 });

            // Act
            await _controller.GenerateTestSaleBuyingProjectReport(parentProject);

            // Assert
            await _bosworthInterfaceService.Received(1).GetTestSaleBuyingProjectAsync(parentProject);
        }

        [Fact]
        public async Task GenerateTestSaleBuyingProjectReport_WhenDataIsNull_PassesEmptyListToExcel()
        {
            // Arrange
            var parentProject = "PP1";
            var response = new ApiResponseDto<List<TestSaleBuyingProjectDto>> { Success = false, Data = null };
            _bosworthInterfaceService.GetTestSaleBuyingProjectAsync(parentProject).Returns(response);
            _excelExportService.ExportToExcel(Arg.Any<IEnumerable<TestSaleBuyingProjectDto>>(), "TestSaleBuyingProject")
                .Returns(new byte[] { 1 });

            // Act
            var result = await _controller.GenerateTestSaleBuyingProjectReport(parentProject);

            // Assert
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1).ExportToExcel(
                Arg.Is<IEnumerable<TestSaleBuyingProjectDto>>(d => !d.Any()), "TestSaleBuyingProject");
        }

        #endregion
    }
}
