using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Api.UnitTests.Controller.ProjectsControllerTest
{
    public class ProjectsControllerTests
    {
        private readonly IProjectService _projectService;
        private readonly IContractService _contractService;
        private readonly IDiseaseService _diseaseService;
        private readonly IProgramService _programService;
        private readonly ICustomerService _customerService;
        private readonly IStaffService _staffService;
        private readonly IMapper _mapper;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _projectService = Substitute.For<IProjectService>();
            _contractService = Substitute.For<IContractService>();
            _diseaseService = Substitute.For<IDiseaseService>();
            _programService = Substitute.For<IProgramService>();
            _customerService = Substitute.For<ICustomerService>();
            _staffService = Substitute.For<IStaffService>();
            _mapper = Substitute.For<IMapper>();

            _controller = new ProjectsController(
                _projectService,
                _contractService,
                _diseaseService,
                _programService,
                _customerService,
                _staffService,
                _mapper);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_ReturnsOkResult_WithMappedPagination()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1 };
            var filter = new QueryParameters<string> { Page = 1 };
            var paginatedResult = new PaginatedResult<ProjectDto>();
            var mappedResult = new PaginationRes<ProjectRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _projectService.GetPaginatedProjectsAsync(filter).Returns(paginatedResult);
            _mapper.Map<PaginationRes<ProjectRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaginatedProjectsAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _projectService.Received(1).GetPaginatedProjectsAsync(filter);
            _mapper.Received(1).Map<PaginationRes<ProjectRes>>(paginatedResult);
        }


        [Fact]
        public async Task GetProject_WithValidId_ReturnsOkResult_WithMappedProject()
        {
            // Arrange
            var projectId = "123";
            var projectDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Test Project" };
            var projectRes = new ProjectRes { ProjectId = projectId, ProjectTitle = "Test Project" };

            _projectService.GetProjectByIdAsync(projectId).Returns(projectDto);
            _mapper.Map<ProjectRes>(projectDto).Returns(projectRes);

            // Act
            var result = await _controller.GetProject(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projectRes, okResult.Value);
            await _projectService.Received(1).GetProjectByIdAsync(projectId);
            _mapper.Received(1).Map<ProjectRes>(projectDto);
        }

        [Fact]
        public async Task GetProject_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var projectId = "invalid";
            _projectService.GetProjectByIdAsync(projectId).Returns((ProjectDto?)null);

            // Act
            var result = await _controller.GetProject(projectId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            await _projectService.Received(1).GetProjectByIdAsync(projectId);
            _mapper.DidNotReceive().Map<ProjectRes>(Arg.Any<ProjectDto>());
        }

        [Fact]
        public async Task AddProject_ReturnsCreatedAtAction_WithMappedProject()
        {
            // Arrange
            var projectReq = new ProjectReq { ProjectId = "123", ProjectTitle = "New Project" };
            var projectDto = new ProjectDto { ProjectId = "123", ProjectTitle = "New Project" };
            var resultDto = new ProjectDto { ProjectId = "123", ProjectTitle = "New Project" };
            var projectRes = new ProjectRes { ProjectId = "123", ProjectTitle = "New Project" };

            _mapper.Map<ProjectDto>(projectReq).Returns(projectDto);
            _projectService.AddProjectAsync(projectDto).Returns(resultDto);
            _mapper.Map<ProjectRes>(resultDto).Returns(projectRes);

            // Act
            var result = await _controller.AddProject(projectReq);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetProject), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal("123", createdResult.RouteValues["id"]);
            Assert.Equal(projectRes, createdResult.Value);

            _mapper.Received(1).Map<ProjectDto>(projectReq);
            await _projectService.Received(1).AddProjectAsync(projectDto);
            _mapper.Received(1).Map<ProjectRes>(resultDto);
        }

        [Fact]
        public async Task UpdateProject_ReturnsOkResult_WithMappedProject()
        {
            // Arrange
            var projectId = "123";
            var projectReq = new ProjectReq { ProjectId = projectId, ProjectTitle = "Updated Project" };
            var projectDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Updated Project" };
            var resultDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Updated Project" };
            var projectRes = new ProjectRes { ProjectId = projectId, ProjectTitle = "Updated Project" };

            _mapper.Map<ProjectDto>(projectReq).Returns(projectDto);
            _projectService.UpdateProjectAsync(projectId, projectDto).Returns(resultDto);
            _mapper.Map<ProjectRes>(resultDto).Returns(projectRes);

            // Act
            var result = await _controller.UpdateProject(projectId, projectReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projectRes, okResult.Value);

            _mapper.Received(1).Map<ProjectDto>(projectReq);
            await _projectService.Received(1).UpdateProjectAsync(projectId, projectDto);
            _mapper.Received(1).Map<ProjectRes>(resultDto);
        }

        // NEW TEST: UpdateProject with Exception
        [Fact]
        public async Task UpdateProject_WithException_ThrowsException()
        {
            // Arrange
            var projectId = "123";
            var projectReq = new ProjectReq { ProjectId = projectId, ProjectTitle = "Updated Project" };
            var projectDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Updated Project" };

            _mapper.Map<ProjectDto>(projectReq).Returns(projectDto);
            _projectService.UpdateProjectAsync(projectId, projectDto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateProject(projectId, projectReq));

            _mapper.Received(1).Map<ProjectDto>(projectReq);
            await _projectService.Received(1).UpdateProjectAsync(projectId, projectDto);
        }

        // UPDATED TEST: DeleteProject - Now returns Ok(true) or throws ArgumentException
        [Fact]
        public async Task DeleteProject_WithValidId_ReturnsOkTrue()
        {
            // Arrange
            var projectId = "123";
            _projectService.DeleteProjectAsync(projectId).Returns(true);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True(okResult.Value is bool b && b);
            await _projectService.Received(1).DeleteProjectAsync(projectId);
        }

        // UPDATED TEST: DeleteProject - Service returns false, throws ArgumentException
        [Fact]
        public async Task DeleteProject_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            var projectId = "invalid";
            _projectService.DeleteProjectAsync(projectId).Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProject(projectId));
            Assert.Equal("Error while deleting project", exception.Message);
            await _projectService.Received(1).DeleteProjectAsync(projectId);
        }

        // NEW TEST: DeleteProject with null/empty ID throws ArgumentException
        [Fact]
        public async Task DeleteProject_WithNullOrEmptyId_ThrowsArgumentException()
        {
            // Act & Assert for null
            var exceptionNull = await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProject(null!));
            Assert.Equal("Project not found for deletion", exceptionNull.Message);

            // Act & Assert for empty string
            var exceptionEmpty = await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProject(""));
            Assert.Equal("Project not found for deletion", exceptionEmpty.Message);
        }

        // UPDATED TEST: CopyProject now returns CreatedAtAction
        [Fact]
        public async Task CopyProject_ReturnsCreatedAtAction_WithMappedProject()
        {
            // Arrange
            var sourceId = "123";
            var newId = "456";
            var resultDto = new ProjectDto { ProjectId = newId, ProjectTitle = "Copied Project" };
            var projectRes = new ProjectRes { ProjectId = newId, ProjectTitle = "Copied Project" };

            _projectService.CopyProjectAsync(sourceId, newId).Returns(resultDto);
            _mapper.Map<ProjectRes>(resultDto).Returns(projectRes);

            // Act
            var result = await _controller.CopyProject(sourceId, newId);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetProject), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(newId, createdResult.RouteValues["id"]);
            Assert.Equal(projectRes, createdResult.Value);

            await _projectService.Received(1).CopyProjectAsync(sourceId, newId);
            _mapper.Received(1).Map<ProjectRes>(resultDto);
        }

        // NEW TEST: CopyProject with Exception
        [Fact]
        public async Task CopyProject_WithException_ThrowsException()
        {
            // Arrange
            var sourceId = "123";
            var newId = "456";

            _projectService.CopyProjectAsync(sourceId, newId).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CopyProject(sourceId, newId));

            await _projectService.Received(1).CopyProjectAsync(sourceId, newId);
        }

        [Fact]
        public async Task RecostProject_ReturnsOkResult_WithSuccessFlag()
        {
            // Arrange
            var projectId = "123";
            _projectService.RecostProjectAsync(projectId).Returns(true);

            // Act
            var result = await _controller.RecostProject(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.True((bool)okResult.Value);
            await _projectService.Received(1).RecostProjectAsync(projectId);
        }

        [Fact]
        public async Task RecostProject_ReturnsOkResult_WithFailureFlag()
        {
            // Arrange
            var projectId = "123";
            _projectService.RecostProjectAsync(projectId).Returns(false);

            // Act
            var result = await _controller.RecostProject(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.False((bool)okResult.Value);
            await _projectService.Received(1).RecostProjectAsync(projectId);
        }

        // UPDATED TESTS: GetNextProjectNumber now returns ApiResponse<string>
        [Fact]
        public async Task GetNextProjectNumber_WithBaseNumber_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var baseNumber = "PRJ";
            var nextNumber = "PRJ-001";
            _projectService.GetNextProjectNumberAsync(baseNumber).Returns(nextNumber);

            // Act
            var result = await _controller.GetNextProjectNumber(baseNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(nextNumber, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _projectService.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        [Fact]
        public async Task GetNextProjectNumber_WithNullBaseNumber_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var nextNumber = "001";
            _projectService.GetNextProjectNumberAsync(null).Returns(nextNumber);

            // Act
            var result = await _controller.GetNextProjectNumber(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<string>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(nextNumber, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _projectService.Received(1).GetNextProjectNumberAsync(null);
        }

        // NEW TEST: GetNextProjectNumber with exception
        [Fact]
        public async Task GetNextProjectNumber_WithException_ThrowsException()
        {
            // Arrange
            var baseNumber = "PRJ";
            _projectService.GetNextProjectNumberAsync(baseNumber).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetNextProjectNumber(baseNumber));

            await _projectService.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        // UPDATED TESTS: GetContracts now returns ApiResponse<List<ContractRes>>
        [Fact]
        public async Task GetContracts_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var contracts = new List<string> { "Contract1", "Contract2" };

            _contractService.GetAllContractNumbersAsync().Returns(contracts);

            // Act
            var result = await _controller.GetContracts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<ContractRes>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count);
            Assert.Equal("Contract1", response.Data[0].ContractNumber);
            Assert.Equal("Contract2", response.Data[1].ContractNumber);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _contractService.Received(1).GetAllContractNumbersAsync();
        }

        // NEW TEST: GetContracts with exception
        [Fact]
        public async Task GetContracts_WithException_ThrowsException()
        {
            // Arrange
            _contractService.GetAllContractNumbersAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetContracts());

            await _contractService.Received(1).GetAllContractNumbersAsync();
        }

        // UPDATED TESTS: GetDiseases now returns ApiResponse<List<DiseaseRes>>
        [Fact]
        public async Task GetDiseases_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var diseases = new List<DiseaseDto> { new DiseaseDto { DiseaseName = "Disease1" } };
            var diseaseRes = new List<DiseaseRes> { new DiseaseRes { DiseaseName = "Disease1" } };

            _diseaseService.GetAllDiseasesAsync().Returns(diseases);
            _mapper.Map<List<DiseaseRes>>(diseases).Returns(diseaseRes);

            // Act
            var result = await _controller.GetDiseases();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<DiseaseRes>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(diseaseRes, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _diseaseService.Received(1).GetAllDiseasesAsync();
            _mapper.Received(1).Map<List<DiseaseRes>>(diseases);
        }

        // NEW TEST: GetDiseases with exception
        [Fact]
        public async Task GetDiseases_WithException_ThrowsException()
        {
            // Arrange
            _diseaseService.GetAllDiseasesAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetDiseases());

            await _diseaseService.Received(1).GetAllDiseasesAsync();
        }

        // UPDATED TESTS: GetPrograms now returns ApiResponse<List<ProgramRes>>
        [Fact]
        public async Task GetPrograms_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var programs = new List<ProgramDto> { new ProgramDto { ProgramNo = "P1" } };
            var programRes = new List<ProgramRes> { new ProgramRes { ProgramNo = "P1" } };

            _programService.GetAllProgramsAsync().Returns(programs);
            _mapper.Map<List<ProgramRes>>(programs).Returns(programRes);

            // Act
            var result = await _controller.GetPrograms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<ProgramRes>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(programRes, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _programService.Received(1).GetAllProgramsAsync();
            _mapper.Received(1).Map<List<ProgramRes>>(programs);
        }

        // NEW TEST: GetPrograms with exception
        [Fact]
        public async Task GetPrograms_WithException_ThrowsException()
        {
            // Arrange
            _programService.GetAllProgramsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPrograms());

            await _programService.Received(1).GetAllProgramsAsync();
        }

        // UPDATED TESTS: GetCustomers now returns ApiResponse<List<CustomerRes>>
        [Fact]
        public async Task GetCustomers_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var customers = new List<CustomerDto> { new CustomerDto { CustomerName = "Customer1" } };
            var customerRes = new List<CustomerRes> { new CustomerRes { CustomerName = "Customer1" } };

            _customerService.GetAllCustomersAsync().Returns(customers);
            _mapper.Map<List<CustomerRes>>(customers).Returns(customerRes);

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<CustomerRes>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(customerRes, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _customerService.Received(1).GetAllCustomersAsync();
            _mapper.Received(1).Map<List<CustomerRes>>(customers);
        }

        // NEW TEST: GetCustomers with exception
        [Fact]
        public async Task GetCustomers_WithException_ThrowsException()
        {
            // Arrange
            _customerService.GetAllCustomersAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetCustomers());

            await _customerService.Received(1).GetAllCustomersAsync();
        }

        // UPDATED TESTS: GetStaff now returns ApiResponse<List<StaffRes>>
        [Fact]
        public async Task GetStaff_ReturnsOkResult_WithApiResponse()
        {
            // Arrange
            var staff = new List<StaffDto> { new StaffDto { Name = "Staff1" } };
            var staffRes = new List<StaffRes> { new StaffRes { Name = "Staff1" } };

            _staffService.GetAllStaffAsync().Returns(staff);
            _mapper.Map<List<StaffRes>>(staff).Returns(staffRes);

            // Act
            var result = await _controller.GetStaff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<StaffRes>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(staffRes, response.Data);
            Assert.NotNull(response.Errors);
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Meta);

            await _staffService.Received(1).GetAllStaffAsync();
            _mapper.Received(1).Map<List<StaffRes>>(staff);
        }

        // NEW TEST: GetStaff with exception
        [Fact]
        public async Task GetStaff_WithException_ThrowsException()
        {
            // Arrange
            _staffService.GetAllStaffAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaff());

            await _staffService.Received(1).GetAllStaffAsync();
        }

        // Add after GetPaginatedProjectsAsync_ReturnsOkResult_WithMappedPagination test
        [Fact]
        public async Task GetPaginatedProjectsAsync_WithException_ThrowsException()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1 };
            var filter = new QueryParameters<string> { Page = 1 };

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _projectService.GetPaginatedProjectsAsync(filter).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaginatedProjectsAsync(query));

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _projectService.Received(1).GetPaginatedProjectsAsync(filter);
        }

        // Add after AddProject_ReturnsCreatedAtAction_WithMappedProject test
        [Fact]
        public async Task AddProject_WithException_ThrowsException()
        {
            // Arrange
            var projectReq = new ProjectReq { ProjectId = "123", ProjectTitle = "New Project" };
            var projectDto = new ProjectDto { ProjectId = "123", ProjectTitle = "New Project" };

            _mapper.Map<ProjectDto>(projectReq).Returns(projectDto);
            _projectService.AddProjectAsync(projectDto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.AddProject(projectReq));

            _mapper.Received(1).Map<ProjectDto>(projectReq);
            await _projectService.Received(1).AddProjectAsync(projectDto);
        }

        // Add after RecostProject_ReturnsOkResult_WithFailureFlag test
        [Fact]
        public async Task RecostProject_WithException_ThrowsException()
        {
            // Arrange
            var projectId = "123";
            _projectService.RecostProjectAsync(projectId).Throws(new Exception("Calculation error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.RecostProject(projectId));

            await _projectService.Received(1).RecostProjectAsync(projectId);
        }
    }
}
