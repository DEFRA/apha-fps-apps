using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.SubContractControllerTest
{
    public class SubContractControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectSubContractService _subContractService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly SubContractController _controller;

        public SubContractControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _subContractService = Substitute.For<IProjectSubContractService>();
            _projectService = Substitute.For<IProjectService>();
            _monthService = Substitute.For<IMonthService>();
            _controller = new SubContractController(
                _mapper,
                _subContractService,
                _projectService,
                _monthService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupSubContractsGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<SubContractItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
        }

        private void SetupProjectsList(List<ProjectDto> projects)
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
        }

        private void SetupMonthsList(List<MonthDto> months)
        {
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(months));
        }

        #region Index

        [Fact]
        public async Task Index_WithParentProjectAndMonth_ReturnsViewWithFilteredViewModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            const int month = 6;
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" }
            };
            var subContracts = new List<ProjectSubContractDto>
            {
                new() { Project = parentProject, Month = month, Amount = 1000m }
            };
            var subContractItems = new List<SubContractItem>
            {
                new() { Project = parentProject, Month = month, Amount = 1000m }
            };

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(subContracts, new PaginationDto()));
            SetupProjectsList(projects);
            SetupMonthsList([]);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<SubContractItem>>(subContracts)
                .Returns(subContractItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(parentProject, month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.ParentProject);
            Assert.Equal(month, model.Month);
            Assert.NotNull(model.FilterProjects);
        }

        [Fact]
        public async Task Index_WithNullParameters_ReturnsViewWithEmptyFilters()
        {
            // Arrange
            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ParentProject);
            Assert.Null(model.Month);
        }

        [Fact]
        public async Task Index_PopulatesProjectsList_InViewBagAndModel()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" },
                new() { ParentProject = "PRJ003" }
            };

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList(projects);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.NotNull(_controller.ViewBag.Projects);
            Assert.NotNull(_controller.ViewBag.FilterProjects);
            Assert.NotEmpty(model.FilterProjects);
        }

        [Fact]
        public async Task Index_PopulatesMonthsList_InModel()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 1, Monthname = "January" },
                new() { Monthnumber = 2, Monthname = "February" }
            };

            SetupProjectsList([]);
            SetupMonthsList(months);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.NotEmpty(model.FilterMonths);
        }

        [Fact]
        public async Task Index_WithMonthFilter_AppliesMonthToDefaultRequest()
        {
            // Arrange
            const int month = 5;
            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, month);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Equal(month, model.Month);
        }

        #endregion

        #region LoadSubContractsGrid

        [Fact]
        public async Task LoadSubContractsGrid_ValidRequest_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string parentProject = "PRJ001";

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, parentProject, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<SubContractItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadSubContractsGrid_WithMonthFilter_MergesMonthIntoFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const int month = 7;

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, null, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"7\"", request.Filter);
        }

        [Fact]
        public async Task LoadSubContractsGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Validation error");

            // Act
            var result = await _controller.LoadSubContractsGrid(new PaginationFilter<string>(), null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadSubContractsGrid_WithExistingFilter_PreservesExistingFilterData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{\"Project\":\"PRJ001\"}" };
            const int month = 3;

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, null, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("Project", request.Filter);
            Assert.Contains("Month", request.Filter);
        }

        [Fact]
        public async Task LoadSubContractsGrid_WithNullFilter_CreatesNewFilterDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            const int month = 4;

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, null, month);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"4\"", request.Filter);
        }

        #endregion

        #region GetSubContract

        [Fact]
        public async Task GetSubContract_IdIsZero_ReturnsPartialViewWithNewSubContract()
        {
            // Arrange
            const string parentProject = "PRJ001";
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetSubContract(0, parentProject);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditSubContract", partial.ViewName);
            var model = Assert.IsType<SubContractItem>(partial.Model);
            Assert.Equal(parentProject, model.Project);
            Assert.Equal(0, model.SubContCounter);
        }

        [Fact]
        public async Task GetSubContract_IdIsZero_WithNullProject_ReturnsPartialViewWithEmptyProject()
        {
            // Arrange
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetSubContract(0, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<SubContractItem>(partial.Model);
            Assert.Equal(string.Empty, model.Project);
        }

        [Fact]
        public async Task GetSubContract_IdIsNonZero_RecordExists_ReturnsPartialViewWithMappedSubContract()
        {
            // Arrange
            const int subContractId = 5;
            var dto = new ProjectSubContractDto
            {
                SubContCounter = subContractId,
                Project = "PRJ001",
                Month = 4,
                Amount = 2500m,
                AcctCode = "ACC001",
                WorkGroup = "WG01",
                TestJob = "TJ01",
                Supplier = "Supplier A",
                SupplierNumber = 123,
                Description = "Test description"
            };
            var viewModel = new SubContractItem
            {
                SubContCounter = subContractId,
                Project = "PRJ001",
                Month = 4,
                Amount = 2500m,
                AcctCode = "ACC001",
                WorkGroup = "WG01",
                TestJob = "TJ01",
                Supplier = "Supplier A",
                SupplierNumber = 123,
                Description = "Test description"
            };

            _subContractService.GetByIdAsync(subContractId)
                .Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));
            _mapper.Map<SubContractItem>(dto).Returns(viewModel);
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetSubContract(subContractId, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditSubContract", partial.ViewName);
            var model = Assert.IsType<SubContractItem>(partial.Model);
            Assert.Equal(subContractId, model.SubContCounter);
            Assert.Equal("PRJ001", model.Project);
        }

        [Fact]
        public async Task GetSubContract_IdIsNonZero_ServiceReturnsFailure_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "SubContract not found" } };
            _subContractService.GetByIdAsync(99)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetSubContract(99, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetSubContract_IdIsNonZero_DataIsNull_ReturnsNotFound()
        {
            // Arrange
            _subContractService.GetByIdAsync(10)
                .Returns(new ApiResponseDto<ProjectSubContractDto> { Success = true, Data = null });
            SetupProjectsList([]);
            SetupMonthsList([]);

            // Act
            var result = await _controller.GetSubContract(10, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetSubContract_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Invalid ID");

            // Act
            var result = await _controller.GetSubContract(0, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetSubContract_PopulatesViewBagWithProjectsAndMonths()
        {
            // Arrange
            var projects = new List<ProjectDto> { new() { ParentProject = "PRJ001" } };
            var months = new List<MonthDto> { new() { Monthnumber = 1, Monthname = "January" } };

            SetupProjectsList(projects);
            SetupMonthsList(months);

            // Act
            var result = await _controller.GetSubContract(0, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Projects);
            Assert.NotNull(_controller.ViewBag.Months);
        }

        #endregion

        #region SaveSubContract

        [Fact]
        public async Task SaveSubContract_CounterIsZero_CallsCreateService_ReturnsSuccessJson()
        {
            // Arrange
            var model = new SubContractItem
            {
                SubContCounter = 0,
                Project = "PRJ001",
                Month = 6,
                Amount = 1500m,
                AcctCode = "ACC001",
                WorkGroup = "WG01",
                TestJob = "TJ01",
                Supplier = "Supplier A",
                SupplierNumber = 123,
                Description = "Test subcontract"
            };
            var dto = new ProjectSubContractDto { SubContCounter = 0 };

            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Sub Contract saved successfully.", value.GetProperty("message").GetString());
            await _subContractService.Received(1).CreateAsync(dto);
            await _subContractService.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<ProjectSubContractDto>());
        }

        [Fact]
        public async Task SaveSubContract_CounterIsNonZero_CallsUpdateService_ReturnsSuccessJson()
        {
            // Arrange
            var model = new SubContractItem
            {
                SubContCounter = 10,
                Project = "PRJ002",
                Month = 8,
                Amount = 3000m
            };
            var dto = new ProjectSubContractDto { SubContCounter = 10 };

            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.UpdateAsync(10, dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("SubContract updated successfully.", value.GetProperty("message").GetString());
            await _subContractService.Received(1).UpdateAsync(10, dto);
            await _subContractService.DidNotReceive().CreateAsync(Arg.Any<ProjectSubContractDto>());
        }

        [Fact]
        public async Task SaveSubContract_ServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            var model = new SubContractItem { SubContCounter = 0, Project = "PRJ001", Month = 1 };
            var dto = new ProjectSubContractDto();
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERR001", Message = "Failed to save subcontract" }
            };

            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to save subcontract.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveSubContract_InvalidModelState_ReturnsValidationErrorJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Month", "Month is required");
            _controller.ModelState.AddModelError("Amount", "Amount must be positive");

            // Act
            var result = await _controller.SaveSubContract(new SubContractItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            Assert.True(value.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task SaveSubContract_ModelStateErrorWithDollarPrefix_FiltersOutDollarKeys()
        {
            // Arrange
            _controller.ModelState.AddModelError("$", "Should be ignored");
            _controller.ModelState.AddModelError("Month", "Month is required");

            // Act
            var result = await _controller.SaveSubContract(new SubContractItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal("Month", errors[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task SaveSubContract_ModelStateErrorWithJsonPathPrefix_StripsPrefix()
        {
            // Arrange
            _controller.ModelState.AddModelError("$.Amount", "Amount is invalid");

            // Act
            var result = await _controller.SaveSubContract(new SubContractItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            var errors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Single(errors);
            Assert.Equal("Amount", errors[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task SaveSubContract_ServiceReturnsErrorsWithDetails_ReturnsErrorDetailsInJson()
        {
            // Arrange
            var model = new SubContractItem { SubContCounter = 0 };
            var dto = new ProjectSubContractDto();
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "Project", Message = "Project is required" },
                new() { Code = "Amount", Message = "Amount must be greater than zero" }
            };

            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveSubContract(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var returnedErrors = value.GetProperty("errors").EnumerateArray().ToList();
            Assert.Equal(2, returnedErrors.Count);
        }

        #endregion

        #region DeleteSubContract

        [Fact]
        public async Task DeleteSubContract_ServiceSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            const int subContractId = 15;
            _subContractService.DeleteAsync(subContractId)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteSubContract(subContractId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _subContractService.Received(1).DeleteAsync(subContractId);
        }

        [Fact]
        public async Task DeleteSubContract_ServiceFails_ReturnsFailureJson()
        {
            // Arrange
            const int subContractId = 20;
            var errors = new List<ApiErrorDto> { new() { Message = "Cannot delete subcontract" } };
            _subContractService.DeleteAsync(subContractId)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteSubContract(subContractId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete subcontract.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteSubContract_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Invalid ID");

            // Act
            var result = await _controller.DeleteSubContract(0);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetProjectsListAsync (Private Method Testing via Index)

        [Fact]
        public async Task Index_ProjectServiceReturnsEmpty_ViewModelHasEmptyProjectsList()
        {
            // Arrange
            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Empty(model.FilterProjects);
        }

        [Fact]
        public async Task Index_ProjectServiceReturnsProjects_ViewModelHasOrderedProjectsList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ003" },
                new() { ParentProject = "PRJ001" },
                new() { ParentProject = "PRJ002" }
            };

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList(projects);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Equal(3, model.FilterProjects.Count);
            Assert.Equal("PRJ001", model.FilterProjects[0].Value);
            Assert.Equal("PRJ002", model.FilterProjects[1].Value);
            Assert.Equal("PRJ003", model.FilterProjects[2].Value);
        }

        [Fact]
        public async Task Index_ProjectServiceFails_ViewModelHasEmptyProjectsList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error" } };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Empty(model.FilterProjects);
        }

        [Fact]
        public async Task Index_ProjectServiceReturnsNull_ViewModelHasEmptyProjectsList()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(new ApiResponseDto<List<ProjectDto>> { Success = true, Data = null });
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Empty(model.FilterProjects);
        }

        #endregion

        #region GetMonthsListAsync (Private Method Testing via Index)

        [Fact]
        public async Task Index_MonthServiceReturnsEmpty_ViewModelHasEmptyMonthsList()
        {
            // Arrange
            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Empty(model.FilterMonths);
        }

        [Fact]
        public async Task Index_MonthServiceReturnsMonths_ViewModelHasOrderedMonthsList()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 3, Monthname = "March" },
                new() { Monthnumber = 1, Monthname = "January" },
                new() { Monthnumber = 2, Monthname = "February" }
            };

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList(months);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Equal(3, model.FilterMonths.Count);
            Assert.Equal("1", model.FilterMonths[0].Value);
            Assert.Equal("2", model.FilterMonths[1].Value);
            Assert.Equal("3", model.FilterMonths[2].Value);
        }

        [Fact]
        public async Task Index_MonthServiceFails_ViewModelHasEmptyMonthsList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Month service error" } };
            _monthService.GetAllMonthsAsync()
                .Returns(ApiResponseDto<List<MonthDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupProjectsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Empty(model.FilterMonths);
        }

        [Fact]
        public async Task Index_MonthServiceReturnsNull_ViewModelHasEmptyMonthsList()
        {
            // Arrange
            _monthService.GetAllMonthsAsync()
                .Returns(new ApiResponseDto<List<MonthDto>> { Success = true, Data = null });
            SetupProjectsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.Empty(model.FilterMonths);
        }

        #endregion

        #region BuildSubContractGridAsync (Private Method Testing via Index)

        [Fact]
        public async Task Index_BuildsGridWithCorrectConfiguration()
        {
            // Arrange
            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsList([]);
            SetupMonthsList([]);
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractViewModel>(viewResult.Model);
            Assert.NotNull(model.SubContractsGrid);
            Assert.Equal("subContractsGrid", model.SubContractsGrid.GridId);
            Assert.Equal("SubContCounter", model.SubContractsGrid.KeyProperty);
        }

        [Fact]
        public async Task LoadSubContractsGrid_WithParentProject_BuildsUrlWithProjectParameter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string parentProject = "PRJ001";

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, parentProject, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SubContractItem>>(partial.Model);
            Assert.Contains($"parentProject={Uri.EscapeDataString(parentProject)}", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadSubContractsGrid_WithMonthAndProject_BuildsUrlWithBothParameters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string parentProject = "PRJ001";
            const int month = 6;

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            SetupSubContractsGridMapper();

            // Act
            var result = await _controller.LoadSubContractsGrid(request, parentProject, month);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SubContractItem>>(partial.Model);
            Assert.Contains($"parentProject={Uri.EscapeDataString(parentProject)}", gridConfig.BindGridUrl);
            Assert.Contains($"month={month}", gridConfig.BindGridUrl);
        }

        #endregion
    }
}

