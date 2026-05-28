using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProposedProjectControllerTest
{
    public class ProposedProjectControllerTests
    {
        private readonly IProposedProjectService _service;
        private readonly IMapper _mapper;
        private readonly ProposedProjectController _controller;

        public ProposedProjectControllerTests()
        {
            _service = Substitute.For<IProposedProjectService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProposedProjectController(_service, _mapper);
        }

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

        #region CreateProposedProject

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
            _service.AddProposedProjectAsync(dto).Returns(createdDto);
            _mapper.Map<ProposedProjectRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.CreateProposedProject(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetProposedProjectById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal("PP001", createdResult.RouteValues["parentproject"]);
            Assert.Equal(createdRes, createdResult.Value);

            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).AddProposedProjectAsync(dto);
            _mapper.Received(1).Map<ProposedProjectRes>(createdDto);
        }

        [Fact]
        public async Task CreateProject_ReturnsCreatedAtAction_WithCorrectRouteValues()
        {
            // Arrange
            var request = new ProposedProjectReq { Parentproject = "PP999", Projecttitle = "Another Project" };
            var dto = new ProposedProjectDto { Parentproject = "PP999", Projecttitle = "Another Project" };
            var createdDto = new ProposedProjectDto { Id = 10, Parentproject = "PP999", Projecttitle = "Another Project" };
            var createdRes = new ProposedProjectRes { Id = 10, Parentproject = "PP999", Projecttitle = "Another Project" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.AddProposedProjectAsync(dto).Returns(createdDto);
            _mapper.Map<ProposedProjectRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.CreateProposedProject(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("PP999", createdResult.RouteValues!["parentproject"]);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new ProposedProjectReq { Parentproject = "PP001", Projecttitle = "New Project" };
            var dto = new ProposedProjectDto { Parentproject = "PP001", Projecttitle = "New Project" };

            _mapper.Map<ProposedProjectDto>(request).Returns(dto);
            _service.AddProposedProjectAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateProposedProject(request));

            _mapper.Received(1).Map<ProposedProjectDto>(request);
            await _service.Received(1).AddProposedProjectAsync(dto);
            _mapper.DidNotReceive().Map<ProposedProjectRes>(Arg.Any<ProposedProjectDto>());
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
