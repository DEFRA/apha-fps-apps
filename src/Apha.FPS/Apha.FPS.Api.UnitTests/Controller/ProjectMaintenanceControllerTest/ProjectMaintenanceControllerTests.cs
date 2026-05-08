using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ProjectMaintenanceControllerTest
{
    public class ProjectMaintenanceControllerTests
    {
        private readonly IProjectService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectController _projectController;

        public ProjectMaintenanceControllerTests()
        {
            _serviceMock = Substitute.For<IProjectService>();
            _mapperMock = Substitute.For<IMapper>();
            _projectController = new ProjectController(_serviceMock, _mapperMock);
        }

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_ValidRequest_ReturnsOk()
        {
            var dtos = new List<ProjectDto> { new ProjectDto { ParentProject = "PP001", ProjectTitle = "Test Project" } };
            var mapped = new List<ProjectRes> { new ProjectRes { ParentProject = "PP001", ProjectTitle = "Test Project" } };

            _serviceMock.GetAllProjectsAsync().Returns(dtos);
            _mapperMock.Map<List<ProjectRes>>(dtos).Returns(mapped);

            var result = await _projectController.GetAllProjectsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region GetPagedPactProjectsAsync

        [Fact]
        public async Task GetPagedPactProjectsAsync_ValidQuery_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectDto> { new ProjectDto { ParentProject = "PP001" } };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectDto>(dtos, paginationDto);
            var mapped = new PaginationRes<ProjectRes>
            {
                Data = new List<ProjectRes> { new ProjectRes { ParentProject = "PP001" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedPactProjectsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mapped);

            var result = await _projectController.GetPagedPactProjectsAsync(query);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region GetAllPactProjectsAsync

        [Fact]
        public async Task GetAllPactProjectsAsync_ValidRequest_ReturnsOk()
        {
            var dtos = new List<ProjectDto> { new ProjectDto { ParentProject = "PP001" } };
            var mapped = new List<ProjectRes> { new ProjectRes { ParentProject = "PP001" } };

            _serviceMock.GetAllPactProjectsAsync().Returns(dtos);
            _mapperMock.Map<List<ProjectRes>>(dtos).Returns(mapped);

            var result = await _projectController.GetAllPactProjectsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        #endregion

        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_ExistingProject_ReturnsOk()
        {
            var dto = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _serviceMock.GetProjectByIdAsync("PP001").Returns(dto);
            _mapperMock.Map<ProjectRes>(dto).Returns(mapped);

            var result = await _projectController.GetProjectByIdAsync("PP001");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ProjectNotFound_ReturnsNotFound()
        {
            _serviceMock.GetProjectByIdAsync("PP999").Returns((ProjectDto?)null);

            var result = await _projectController.GetProjectByIdAsync("PP999");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region CreateProjectAsync

        [Fact]
        public async Task CreateProjectAsync_ValidRequest_ReturnsCreatedAtAction()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Test Project" };
            var dto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Test Project" };
            var createdDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Test Project" };
            var mapped = new ProjectRes { ParentProject = "PP001", ProjectTitle = "Test Project" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.CreateProjectAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectRes>(createdDto).Returns(mapped);

            var result = await _projectController.CreateProjectAsync(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(mapped, createdResult.Value);
        }

        [Fact]
        public async Task CreateProjectAsync_ServiceThrows_ThrowsException()
        {
            var req = new ProjectReq { ParentProject = "PP001" };
            var dto = new ProjectDto { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.CreateProjectAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _projectController.CreateProjectAsync(req));
        }

        #endregion

        #region UpdateProjectRootAsync

        [Fact]
        public async Task UpdateProjectRootAsync_ValidRequest_ReturnsOk()
        {
            var req = new ProjectReq { ParentProject = "PP001" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var updatedDto = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdateProjectAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectRes>(updatedDto).Returns(mapped);

            var result = await _projectController.UpdateProjectRootAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdateProjectRootAsync_ServiceThrows_ThrowsException()
        {
            var req = new ProjectReq { ParentProject = "PP001" };
            var dto = new ProjectDto { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdateProjectAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _projectController.UpdateProjectRootAsync(req));
        }

        #endregion

        #region UpdatePactProjectDetailsAsync

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_ValidRequest_ReturnsOk()
        {
            var req = new ProjectReq { ParentProject = "PP001" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var updatedDto = new ProjectDto { ParentProject = "PP001" };
            var mapped = new ProjectRes { ParentProject = "PP001" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdatePactProjectDetailsAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectRes>(updatedDto).Returns(mapped);

            var result = await _projectController.UpdatePactProjectDetailsAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_ProjectNotFound_ReturnsNotFound()
        {
            var req = new ProjectReq { ParentProject = "PP999" };
            var dto = new ProjectDto { ParentProject = "PP999" };

            _mapperMock.Map<ProjectDto>(req).Returns(dto);
            _serviceMock.UpdatePactProjectDetailsAsync(dto).Returns((ProjectDto?)null);

            var result = await _projectController.UpdatePactProjectDetailsAsync(req);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region DeleteProjectAsync

        [Fact]
        public async Task DeleteProjectAsync_ValidId_ReturnsOk()
        {
            _serviceMock.DeleteProjectAsync("PP001").Returns(true);

            var result = await _projectController.DeleteProjectAsync("PP001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteProjectAsync_EmptyId_ReturnsBadRequest()
        {
            var result = await _projectController.DeleteProjectAsync("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_ProjectNotFound_ReturnsNotFound()
        {
            _serviceMock.DeleteProjectAsync("PP999").Returns(false);

            var result = await _projectController.DeleteProjectAsync("PP999");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }

    public class ProjectReqValidationTests
    {
        private static IList<ValidationResult> Validate(ProjectReq req)
        {
            var context = new ValidationContext(req);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(req, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void ProjectReq_WhenValid_PassesValidation()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha Project" };
            Assert.Empty(Validate(req));
        }

        [Fact]
        public void ProjectReq_WhenParentProjectMissing_FailsValidation()
        {
            var req = new ProjectReq { ParentProject = null!, ProjectTitle = "Alpha Project" };
            var results = Validate(req);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProjectReq.ParentProject)));
        }

        [Fact]
        public void ProjectReq_WhenProjectTitleMissing_FailsValidation()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = null! };
            var results = Validate(req);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProjectReq.ProjectTitle)));
        }

        [Fact]
        public void ProjectReq_WhenParentProjectExceedsMaxLength_FailsValidation()
        {
            var req = new ProjectReq { ParentProject = new string('X', 21), ProjectTitle = "Alpha Project" };
            var results = Validate(req);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProjectReq.ParentProject)));
        }

        [Fact]
        public void ProjectReq_WhenProjectTitleExceedsMaxLength_FailsValidation()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = new string('X', 201) };
            var results = Validate(req);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProjectReq.ProjectTitle)));
        }

        [Theory]
        [InlineData(nameof(ProjectReq.Program), 11)]
        [InlineData(nameof(ProjectReq.Customer), 51)]
        [InlineData(nameof(ProjectReq.Manager), 51)]
        [InlineData(nameof(ProjectReq.ProjectStatus), 51)]
        [InlineData(nameof(ProjectReq.Disease), 51)]
        [InlineData(nameof(ProjectReq.Contract), 11)]
        [InlineData(nameof(ProjectReq.ProjectParent), 51)]
        [InlineData(nameof(ProjectReq.OracleProjectCode), 51)]
        [InlineData(nameof(ProjectReq.SubAccountCode), 51)]
        [InlineData(nameof(ProjectReq.ProjectGroup), 51)]
        [InlineData(nameof(ProjectReq.IncomeAccountCode), 51)]
        public void ProjectReq_WhenOptionalStringExceedsMaxLength_FailsValidation(string propertyName, int length)
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha Project" };
            typeof(ProjectReq).GetProperty(propertyName)!.SetValue(req, new string('X', length));
            var results = Validate(req);
            Assert.Contains(results, r => r.MemberNames.Contains(propertyName));
        }

        [Fact]
        public void ProjectReq_WhenIsDefraProjectOutOfRange_FailsValidation()
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha", IsDefraProject = 2 };
            var results = Validate(req);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProjectReq.IsDefraProject)));
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)-1)]
        public void ProjectReq_WhenIsDefraProjectIsValid_PassesValidation(short value)
        {
            var req = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Alpha", IsDefraProject = value };
            Assert.Empty(Validate(req));
        }
    }
}
