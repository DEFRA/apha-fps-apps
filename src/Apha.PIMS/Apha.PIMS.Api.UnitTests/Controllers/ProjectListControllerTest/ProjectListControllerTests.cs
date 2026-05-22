using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectListControllerTest
{
    public class ProjectListControllerTests
    {
        private readonly IProjectListService _service;
        private readonly IMapper _mapper;
        private readonly ProjectListController _controller;

        public ProjectListControllerTests()
        {
            _service = Substitute.For<IProjectListService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectListController(_service, _mapper);
        }

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsOkResult_WithMappedPagination()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<ProjectListViewDto>();
            var mappedResult = new PaginationRes<ProjectListRes>();
            var showWhichProjects = 2;

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetAllProjectsAsync(filter, showWhichProjects).Returns(paginatedResult);
            _mapper.Map<PaginationRes<ProjectListRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllProjectsAsync(query, showWhichProjects);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetAllProjectsAsync(filter, showWhichProjects);
            _mapper.Received(1).Map<PaginationRes<ProjectListRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyServiceResult_ReturnsOkWithEmptyPagination()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResult = new PaginatedResult<ProjectListViewDto>();
            var emptyMapped = new PaginationRes<ProjectListRes>();
            var showWhichProjects = 2;

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetAllProjectsAsync(filter, showWhichProjects).Returns(emptyResult);
            _mapper.Map<PaginationRes<ProjectListRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetAllProjectsAsync(query, showWhichProjects);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetAllProjectsAsync(filter, showWhichProjects);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1 };
            var filter = new QueryParameters<string> { Page = 1 };
            var showWhichProjects = 2;

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetAllProjectsAsync(filter, showWhichProjects).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProjectsAsync(query, showWhichProjects));

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetAllProjectsAsync(filter, showWhichProjects);
        }

        #endregion

        #region GetAllProjectsForDropDownAsync

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_ReturnsOkResult_WithMappedList()
        {
            // Arrange
            var dtoList = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "No" }
            };
            var resList = new List<ProjectListRes>
            {
                new ProjectListRes { Parentproject = "PP001", Program = "PROG1", Customer = "CUST1", OnFps = "Yes" },
                new ProjectListRes { Parentproject = "PP002", Program = "PROG2", Customer = "CUST2", OnFps = "No" }
            };

            _service.GetAllProjectsForDropDownAsync().Returns(dtoList);
            _mapper.Map<List<ProjectListRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetAllProjectsForDropDownAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);

            await _service.Received(1).GetAllProjectsForDropDownAsync();
            _mapper.Received(1).Map<List<ProjectListRes>>(dtoList);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyDtoList = new List<ProjectListViewDto>();
            var emptyResList = new List<ProjectListRes>();

            _service.GetAllProjectsForDropDownAsync().Returns(emptyDtoList);
            _mapper.Map<List<ProjectListRes>>(emptyDtoList).Returns(emptyResList);

            // Act
            var result = await _controller.GetAllProjectsForDropDownAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<ProjectListRes>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetAllProjectsForDropDownAsync();
            _mapper.Received(1).Map<List<ProjectListRes>>(emptyDtoList);
        }

        [Fact]
        public async Task GetAllProjectsForDropDownAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllProjectsForDropDownAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllProjectsForDropDownAsync());

            await _service.Received(1).GetAllProjectsForDropDownAsync();
            _mapper.DidNotReceive().Map<List<ProjectListRes>>(Arg.Any<List<ProjectListViewDto>>());
        }

        #endregion

        #region GetFpsProjectById

        [Fact]
        public async Task GetFpsProjectById_WithValidProject_ReturnsOkResult_WithMappedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var projectDto = new ProjectDto
            {
                Parentproject = parentproject,
                Projecttitle = "FMD Survey",
                Disease = "FMD",
                Contract = "CON001",
                Projectstatus = "Active",
                Shorttitle = "FMD",
                Costbookno = "CB001"
            };
            var projectRes = new ProjectRes
            {
                Parentproject = parentproject,
                Projecttitle = "FMD Survey",
                Disease = "FMD",
                Contract = "CON001",
                Projectstatus = "Active"
            };

            _service.GetFpsProjectByIdAsync(parentproject).Returns(projectDto);
            _mapper.Map<ProjectRes>(projectDto).Returns(projectRes);

            // Act
            var result = await _controller.GetFpsProjectById(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projectRes, okResult.Value);

            await _service.Received(1).GetFpsProjectByIdAsync(parentproject);
            _mapper.Received(1).Map<ProjectRes>(projectDto);
        }

        [Fact]
        public async Task GetFpsProjectById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetFpsProjectByIdAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFpsProjectById(parentproject));

            await _service.Received(1).GetFpsProjectByIdAsync(parentproject);
            _mapper.DidNotReceive().Map<ProjectRes>(Arg.Any<ProjectDto>());
        }

        #endregion

        #region GetProposedProjectById

        [Fact]
        public async Task GetProposedProjectById_WithValidProject_ReturnsOkResult_WithMappedProject()
        {
            // Arrange
            var parentproject = "PP001";
            var proposedDto = new ProposedProjectDto
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "TB Project",
                Program = "PROG1",
                Customer = "CUST1",
                Manager = "MGR1",
                Projectstatus = "Proposed",
                Disease = "TB"
            };
            var proposedRes = new ProposedProjectRes
            {
                Id = 1,
                Parentproject = parentproject,
                Projecttitle = "TB Project",
                Projectstatus = "Proposed"
            };

            _service.GetProposedProjectByIdAsync(parentproject).Returns(proposedDto);
            _mapper.Map<ProposedProjectRes>(proposedDto).Returns(proposedRes);

            // Act
            var result = await _controller.GetProposedProjectById(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(proposedRes, okResult.Value);

            await _service.Received(1).GetProposedProjectByIdAsync(parentproject);
            _mapper.Received(1).Map<ProposedProjectRes>(proposedDto);
        }

        [Fact]
        public async Task GetProposedProjectById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetProposedProjectByIdAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProposedProjectById(parentproject));

            await _service.Received(1).GetProposedProjectByIdAsync(parentproject);
            _mapper.DidNotReceive().Map<ProposedProjectRes>(Arg.Any<ProposedProjectDto>());
        }

        #endregion

        #region GetYearlyDetailsByProject

        [Fact]
        public async Task GetYearlyDetailsByProject_ReturnsOkResult_WithMappedList()
        {
            // Arrange
            var parentproject = "PP001";
            var dtoList = new List<ProjectsDto>
            {
                new ProjectsDto { Year = 2023, Parentproject = parentproject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" },
                new ProjectsDto { Year = 2024, Parentproject = parentproject, Program = "PROG1", Customer = "CUST1", Manager = "MGR1" }
            };
            var resList = new List<ProjectsRes>
            {
                new ProjectsRes { Year = 2023, Parentproject = parentproject, Program = "PROG1" },
                new ProjectsRes { Year = 2024, Parentproject = parentproject, Program = "PROG1" }
            };

            _service.GetYearlyDetailsByProjectAsync(parentproject).Returns(dtoList);
            _mapper.Map<List<ProjectsRes>>(dtoList).Returns(resList);

            // Act
            var result = await _controller.GetYearlyDetailsByProject(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);

            await _service.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
            _mapper.Received(1).Map<List<ProjectsRes>>(dtoList);
        }

        [Fact]
        public async Task GetYearlyDetailsByProject_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var parentproject = "PP001";
            var emptyDtoList = new List<ProjectsDto>();
            var emptyResList = new List<ProjectsRes>();

            _service.GetYearlyDetailsByProjectAsync(parentproject).Returns(emptyDtoList);
            _mapper.Map<List<ProjectsRes>>(emptyDtoList).Returns(emptyResList);

            // Act
            var result = await _controller.GetYearlyDetailsByProject(parentproject);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<ProjectsRes>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
            _mapper.Received(1).Map<List<ProjectsRes>>(emptyDtoList);
        }

        [Fact]
        public async Task GetYearlyDetailsByProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parentproject = "PP001";
            _service.GetYearlyDetailsByProjectAsync(parentproject).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetYearlyDetailsByProject(parentproject));

            await _service.Received(1).GetYearlyDetailsByProjectAsync(parentproject);
            _mapper.DidNotReceive().Map<List<ProjectsRes>>(Arg.Any<List<ProjectsDto>>());
        }

        #endregion

        #region CreateProject

        [Fact]
        public async Task CreateProject_ReturnsCreatedAtAction_WithMappedProposedProject()
        {
            // Arrange
            var request = new ProposedProjectReq
            {
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Projectstatus = "Proposed",
                Disease = "FMD"
            };
            var dto = new ProposedProjectDto
            {
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Projectstatus = "Proposed"
            };
            var createdDto = new ProposedProjectDto
            {
                Id = 42,
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Program = "PROG1",
                Customer = "CUST1",
                Projectstatus = "Proposed"
            };
            var createdRes = new ProposedProjectRes
            {
                Id = 42,
                Parentproject = "PP001",
                Projecttitle = "New Project",
                Projectstatus = "Proposed"
            };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.AddProjectAsync(dto).Returns(createdDto);
            _mapper.Map<ProposedProjectRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetProposedProjectById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal("PP001", createdResult.RouteValues["parentproject"]);
            Assert.Equal(createdRes, createdResult.Value);

            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).AddProjectAsync(dto);
            _mapper.Received(1).Map<ProposedProjectRes>(createdDto);
        }

        [Fact]
        public async Task CreateProject_ReturnsCreatedAtAction_WithCorrectRouteValues()
        {
            // Arrange
            var request = new ProposedProjectReq
            {
                Parentproject = "PP999",
                Projecttitle = "Another Project",
                Program = "PROG2",
                Customer = "CUST2",
                Projectstatus = "Proposed",
                Disease = "TB"
            };
            var dto = new ProposedProjectDto { Parentproject = "PP999", Projecttitle = "Another Project" };
            var createdDto = new ProposedProjectDto { Id = 10, Parentproject = "PP999", Projecttitle = "Another Project" };
            var createdRes = new ProposedProjectRes { Id = 10, Parentproject = "PP999", Projecttitle = "Another Project" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.AddProjectAsync(dto).Returns(createdDto);
            _mapper.Map<ProposedProjectRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetProposedProjectById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal("PP999", createdResult.RouteValues["parentproject"]);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(createdRes, createdResult.Value);
        }

        [Fact]
        public async Task CreateProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new ProposedProjectReq { Parentproject = "PP001", Projecttitle = "New Project" };
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.AddProjectAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateProject(request));

            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).AddProjectAsync(dto);
            _mapper.DidNotReceive().Map<ProposedProjectRes>(Arg.Any<ProposedProjectDto>());
        }

        [Fact]
        public async Task CreateProject_MapsRequestToDto_BeforeCallingService()
        {
            // Arrange
            var request = new ProposedProjectReq
            {
                Parentproject = "PP002",
                Projecttitle = "Mapped Project",
                Program = "PROG3",
                Customer = "CUST3",
                Projectstatus = "Proposed",
                Disease = "BVD"
            };
            var dto = new ProposedProjectDto
            {
                Parentproject = "PP002",
                Projecttitle = "Mapped Project",
                Program = "PROG3",
                Customer = "CUST3",
                Projectstatus = "Proposed",
                Disease = "BVD"
            };
            var createdDto = new ProposedProjectDto { Id = 5, Parentproject = "PP002" };
            var createdRes = new ProposedProjectRes { Id = 5, Parentproject = "PP002" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.AddProjectAsync(dto).Returns(createdDto);
            _mapper.Map<ProposedProjectRes>(createdDto).Returns(createdRes);

            // Act
            await _controller.CreateProject(request);

            // Assert - verify mapper was called with the original request before service call
            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).AddProjectAsync(dto);
        }

        #endregion

        #region GetProjectPrograms

        [Fact]
        public async Task GetProjectPrograms_ReturnsOkResult_WithProgramsList()
        {
            // Arrange
            var programs = new List<string> { "PROG1", "PROG2", "PROG3" };
            _service.GetProjectProgramsAsync().Returns(programs);

            // Act
            var result = await _controller.GetProjectPrograms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(programs, okResult.Value);

            await _service.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task GetProjectPrograms_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyPrograms = new List<string>();
            _service.GetProjectProgramsAsync().Returns(emptyPrograms);

            // Act
            var result = await _controller.GetProjectPrograms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetProjectProgramsAsync();
        }

        [Fact]
        public async Task GetProjectPrograms_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetProjectProgramsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectPrograms());

            await _service.Received(1).GetProjectProgramsAsync();
        }

        #endregion

        #region GetProjectCustomers

        [Fact]
        public async Task GetProjectCustomers_ReturnsOkResult_WithCustomersList()
        {
            // Arrange
            var customers = new List<string> { "CUST1", "CUST2", "CUST3" };
            _service.GetProjectCustomersAsync().Returns(customers);

            // Act
            var result = await _controller.GetProjectCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(customers, okResult.Value);

            await _service.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task GetProjectCustomers_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyCustomers = new List<string>();
            _service.GetProjectCustomersAsync().Returns(emptyCustomers);

            // Act
            var result = await _controller.GetProjectCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetProjectCustomersAsync();
        }

        [Fact]
        public async Task GetProjectCustomers_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetProjectCustomersAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectCustomers());

            await _service.Received(1).GetProjectCustomersAsync();
        }

        #endregion

        #region GetProjectStatuses

        [Fact]
        public async Task GetProjectStatuses_ReturnsOkResult_WithStatusesList()
        {
            // Arrange
            var statuses = new List<string> { "Active", "Proposed", "Closed" };
            _service.GetProjectStatusesAsync().Returns(statuses);

            // Act
            var result = await _controller.GetProjectStatuses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(statuses, okResult.Value);

            await _service.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task GetProjectStatuses_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyStatuses = new List<string>();
            _service.GetProjectStatusesAsync().Returns(emptyStatuses);

            // Act
            var result = await _controller.GetProjectStatuses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(value);

            await _service.Received(1).GetProjectStatusesAsync();
        }

        [Fact]
        public async Task GetProjectStatuses_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetProjectStatusesAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjectStatuses());

            await _service.Received(1).GetProjectStatusesAsync();
        }

        #endregion
    }
}