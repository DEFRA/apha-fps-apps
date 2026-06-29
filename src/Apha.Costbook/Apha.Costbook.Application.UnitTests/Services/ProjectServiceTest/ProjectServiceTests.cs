using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Application.Validation;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _projectService = new ProjectService(_mockRepository, _mockMapper);
        }

        #region GetPaginatedProjectsAsync Tests

        [Fact]
        public async Task GetPaginatedProjectsAsync_ValidParameters_ReturnsPaginatedResult()
        {
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "CONTRACT123"
            };
            var paginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "CONTRACT123"
            };
            var pagedDataProjects = new PagedData<Project>
            {
                Data = new List<Project>
                {
                    new Project { ProjectId = "P001", ProjectTitle = "Test Project 1" },
                    new Project { ProjectId = "P002", ProjectTitle = "Test Project 2" }
                },
                PaginationData = new PaginationData
                {
                    TotalRecords = 2,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1
                }
            };
            var paginatedProjectDtos = new PaginatedResult<ProjectDto>
            {
                Data = new List<ProjectDto>
                {
                    new ProjectDto { ProjectId = "P001", ProjectTitle = "Test Project 1" },
                    new ProjectDto { ProjectId = "P002", ProjectTitle = "Test Project 2" }
                },
                PaginationData = new PaginationDto
                {
                    TotalRecords = 2,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(paginationParams);
            _mockRepository.GetPaginatedProjectsAsync(paginationParams).Returns(pagedDataProjects);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedDataProjects).Returns(paginatedProjectDtos);

            // Act
            var result = await _projectService.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            await _mockRepository.Received(1).GetPaginatedProjectsAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedDataProjects);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_EmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };
            var paginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };
            var pagedDataProjects = new PagedData<Project>
            {
                Data = new List<Project>(),
                PaginationData = new PaginationData
                {
                    TotalRecords = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0
                }
            };
            var paginatedProjectDtos = new PaginatedResult<ProjectDto>
            {
                Data = new List<ProjectDto>(),
                PaginationData = new PaginationDto
                {
                    TotalRecords = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(paginationParams);
            _mockRepository.GetPaginatedProjectsAsync(paginationParams).Returns(pagedDataProjects);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedDataProjects).Returns(paginatedProjectDtos);

            // Act
            var result = await _projectService.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            await _mockRepository.Received(1).GetPaginatedProjectsAsync(paginationParams);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithFilter_ReturnsFilteredResults()
        {
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 5,
                Filter = "John Doe"
            };
            var paginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 5,
                Filter = "John Doe"
            };
            var pagedDataProjects = new PagedData<Project>
            {
                Data = new List<Project>
                {
                    new Project { ProjectId = "P001", ProjectTitle = "Test Project", PreparedBy = "John Doe" }
                },
                PaginationData = new PaginationData
                {
                    TotalRecords = 1,
                    PageNumber = 1,
                    PageSize = 5,
                    TotalPages = 1
                }
            };
            var paginatedProjectDtos = new PaginatedResult<ProjectDto>
            {
                Data = new List<ProjectDto>
                {
                    new ProjectDto { ProjectId = "P001", ProjectTitle = "Test Project", PreparedBy = "John Doe" }
                },
                PaginationData = new PaginationDto
                {
                    TotalRecords = 1,
                    PageNumber = 1,
                    PageSize = 5,
                    TotalPages = 1
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(paginationParams);
            _mockRepository.GetPaginatedProjectsAsync(paginationParams).Returns(pagedDataProjects);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedDataProjects).Returns(paginatedProjectDtos);

            // Act
            var result = await _projectService.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("John Doe", result.Data.First().PreparedBy);
            await _mockRepository.Received(1).GetPaginatedProjectsAsync(paginationParams);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 5)]
        [InlineData(3, 20)]
        public async Task GetPaginatedProjectsAsync_DifferentPageParameters_ReturnsCorrectPage(int pageNumber, int pageSize)
        {
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = pageNumber,
                PageSize = pageSize
            };
            var paginationParams = new PaginationParameters<string>
            {
                Page = pageNumber,
                PageSize = pageSize
            };
            var pagedDataProjects = new PagedData<Project>
            {
                Data = new List<Project>(),
                PaginationData = new PaginationData
                {
                    TotalRecords = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0
                }
            };
            var paginatedProjectDtos = new PaginatedResult<ProjectDto>
            {
                Data = new List<ProjectDto>(),
                PaginationData = new PaginationDto
                {
                    TotalRecords = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(paginationParams);
            _mockRepository.GetPaginatedProjectsAsync(paginationParams).Returns(pagedDataProjects);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedDataProjects).Returns(paginatedProjectDtos);

            // Act
            var result = await _projectService.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.Equal(pageNumber, result.PaginationData.PageNumber);
            Assert.Equal(pageSize, result.PaginationData.PageSize);
            await _mockRepository.Received(1).GetPaginatedProjectsAsync(paginationParams);
        }

        #endregion

        #region GetProjectsAsync Tests

        [Fact]
        public async Task GetProjectsAsync_WithFilters_ReturnsProjectDtos()
        {
            // Arrange
            var contractFilter = "CONTRACT123";
            var submittedByFilter = "John Doe";
            var projects = new List<Project>
            {
                new Project { ProjectId = "P001", ProjectTitle = "Test Project 1" },
                new Project { ProjectId = "P002", ProjectTitle = "Test Project 2" }
            };
            var projectDtos = new List<ProjectDto>
            {
                new ProjectDto { ProjectId = "P001", ProjectTitle = "Test Project 1" },
                new ProjectDto { ProjectId = "P002", ProjectTitle = "Test Project 2" }
            };

            _mockRepository.GetProjectsAsync(contractFilter, submittedByFilter).Returns(projects);
            _mockMapper.Map<IEnumerable<ProjectDto>>(projects).Returns(projectDtos);

            // Act
            var result = await _projectService.GetProjectsAsync(contractFilter, submittedByFilter);

            // Assert
            Assert.Equal(2, result.Count());
            await _mockRepository.Received(1).GetProjectsAsync(contractFilter, submittedByFilter);
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(projects);
        }

        [Fact]
        public async Task GetProjectsAsync_WithoutFilters_ReturnsAllProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { ProjectId = "P001", ProjectTitle = "Test Project 1" }
            };
            var projectDtos = new List<ProjectDto>
            {
                new ProjectDto { ProjectId = "P001", ProjectTitle = "Test Project 1" }
            };

            _mockRepository.GetProjectsAsync(null, null).Returns(projects);
            _mockMapper.Map<IEnumerable<ProjectDto>>(projects).Returns(projectDtos);

            // Act
            var result = await _projectService.GetProjectsAsync(null, null);

            // Assert
            Assert.Single(result);
            await _mockRepository.Received(1).GetProjectsAsync(null, null);
        }

        [Fact]
        public async Task GetProjectsAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var projects = new List<Project>();
            var projectDtos = new List<ProjectDto>();

            _mockRepository.GetProjectsAsync(null, null).Returns(projects);
            _mockMapper.Map<IEnumerable<ProjectDto>>(projects).Returns(projectDtos);

            // Act
            var result = await _projectService.GetProjectsAsync(null, null);

            // Assert
            Assert.Empty(result);
            await _mockRepository.Received(1).GetProjectsAsync(null, null);
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(projects);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_ExistingProject_ReturnsProjectDto()
        {
            // Arrange
            var projectId = "P001";
            var project = new Project { ProjectId = projectId, ProjectTitle = "Test Project" };
            var projectDto = new ProjectDto { ProjectId = projectId, ProjectTitle = "Test Project" };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(project);
            _mockMapper.Map<ProjectDto>(project).Returns(projectDto);

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, result.ProjectId);
            await _mockRepository.Received(1).GetProjectByIdAsync(projectId);
            _mockMapper.Received(1).Map<ProjectDto>(project);
        }

        [Fact]
        public async Task GetProjectByIdAsync_NonExistingProject_ReturnsNull()
        {
            // Arrange
            var projectId = "P999";
            _mockRepository.GetProjectByIdAsync(projectId).Returns((Project?)null);

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetProjectByIdAsync(projectId);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region AddProjectAsync Tests

        [Fact]
        public async Task AddProjectAsync_ValidProject_ReturnsProjectDto()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = "Test Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1,
                Inflation = 2,
                FinancialYears = 1
            };

            var project = new Project { ProjectId = "P001", ProjectTitle = "Test Project" };
            var resultProject = new Project { ProjectId = "P001", ProjectTitle = "Test Project" };
            var resultDto = new ProjectDto { ProjectId = "P001", ProjectTitle = "Test Project" };

            _mockMapper.Map<Project>(projectDto).Returns(project);
            _mockRepository.AddProjectAsync(project).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.AddProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProjectId);
            Assert.Equal(2024, projectDto.StartFYear); // Financial year calculated
            await _mockRepository.Received(1).AddProjectAsync(project);
        }

        [Fact]
        public async Task AddProjectAsync_EmptyProjectId_GeneratesNewId()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "",
                ProjectTitle = "Test Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1
            };

            var generatedId = "P001";
            var project = new Project { ProjectId = generatedId };
            var resultProject = new Project { ProjectId = generatedId };
            var resultDto = new ProjectDto { ProjectId = generatedId };

            _mockRepository.GetNextProjectNumberAsync("").Returns(generatedId);
            _mockMapper.Map<Project>(projectDto).Returns(project);
            _mockRepository.AddProjectAsync(project).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.AddProjectAsync(projectDto);

            // Assert
            Assert.Equal(generatedId, projectDto.ProjectId);
            await _mockRepository.Received(1).GetNextProjectNumberAsync("");
        }

        [Fact]
        public async Task AddProjectAsync_NullProjectId_GeneratesNewId()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = null!,
                ProjectTitle = "Test Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1
            };

            var generatedId = "P001";
            var project = new Project { ProjectId = generatedId };
            var resultProject = new Project { ProjectId = generatedId };
            var resultDto = new ProjectDto { ProjectId = generatedId };

            _mockRepository.GetNextProjectNumberAsync("").Returns(generatedId);
            _mockMapper.Map<Project>(projectDto).Returns(project);
            _mockRepository.AddProjectAsync(project).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.AddProjectAsync(projectDto);

            // Assert
            Assert.Equal(generatedId, projectDto.ProjectId);
            await _mockRepository.Received(1).GetNextProjectNumberAsync("");
        }

        [Fact]
        public async Task AddProjectAsync_NullInflation_SetsDefaultInflation()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = "Test Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1,
                Inflation = null
            };

            var project = new Project();
            var resultProject = new Project();
            var resultDto = new ProjectDto();

            _mockMapper.Map<Project>(projectDto).Returns(project);
            _mockRepository.AddProjectAsync(project).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            await _projectService.AddProjectAsync(projectDto);

            // Assert
            Assert.Equal(1, projectDto.Inflation);
        }

        [Fact]
        public async Task AddProjectAsync_InvalidProject_ThrowsArgumentException()
        {

            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = "", // Missing title
                PreparedBy = "John Doe",
                Startdate = null, // Missing start date
                IsDefraProject = null // Missing defra project flag
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.AddProjectAsync(projectDto));
            Assert.Contains(exception.Errors, e => e.Message == "Please enter Start Date");
            Assert.Contains(exception.Errors, e => e.Message == "Please enter a title");
            Assert.Contains(exception.Errors, e => e.Message == "Please choose Defra/Non-Defra");
        }

        [Fact]
        public async Task AddProjectAsync_TitleTooLong_ThrowsArgumentException()
        {
            // Arrange
            var longTitle = new string('A', 101); // 101 characters - exceeds 100 char limit
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = longTitle,
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.AddProjectAsync(projectDto));
            Assert.Contains(exception.Errors, e => e.Message == "Please enter a title of less than 100 characters");
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_ValidProject_ReturnsUpdatedProject()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project
            {
                ProjectId = projectId,
                Inflation = 1,
                IsDefraProject = 0
            };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Updated Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1,
                Inflation = 2
            };

            var resultProject = new Project { ProjectId = projectId };
            var resultDto = new ProjectDto { ProjectId = projectId };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);
            _mockRepository.UpdateProjectAsync(existingProject).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.UpdateProjectAsync(projectId, projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, existingProject.ProjectId);
            await _mockRepository.Received(1).GetProjectByIdAsync(projectId);
            await _mockRepository.Received(1).UpdateProjectAsync(existingProject);
        }

        [Fact]
        public async Task UpdateProjectAsync_ProjectNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectId = "P999";
            var projectDto = new ProjectDto { ProjectId = projectId };

            _mockRepository.GetProjectByIdAsync(projectId).Returns((Project?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _projectService.UpdateProjectAsync(projectId, projectDto));
            Assert.Equal("Project not found", exception.Message);
        }

        [Fact]
        public async Task UpdateProjectAsync_InvalidProject_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project
            {
                ProjectId = projectId,
                Inflation = 1,
                IsDefraProject = 0
            };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "", // Invalid title
                PreparedBy = "", // Invalid prepared by
                Startdate = null, // Invalid start date
                IsDefraProject = null // Invalid defra project flag
            };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.UpdateProjectAsync(projectId, projectDto));
            Assert.Equal(4, exception.Errors.Count);
            Assert.Contains(exception.Errors, e => e.Message == "Please enter Start Date");
            Assert.Contains(exception.Errors, e => e.Message == "Please enter a title");
            Assert.Contains(exception.Errors, e => e.Message == "Please choose Defra/Non-Defra");
            Assert.Contains(exception.Errors, e => e.Message == "Please enter who has prepared this");
        }

        [Fact]
        public async Task UpdateProjectAsync_TitleTooLong_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project { ProjectId = projectId };
            var longTitle = new string('A', 101); // 101 characters - exceeds 100 char limit
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = longTitle,
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1
            };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.UpdateProjectAsync(projectId, projectDto));
            Assert.Single(exception.Errors);
            Assert.Contains(exception.Errors, e => e.Message == "Please enter a title of less than 100 characters");
        }

        [Fact]
        public async Task UpdateProjectAsync_NotesTooLong_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project { ProjectId = projectId };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Valid Title",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1,
                Notes = new string('A', 256) // 256 characters - exceeds 255 char limit
            };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.UpdateProjectAsync(projectId, projectDto));
            Assert.Single(exception.Errors);
            Assert.Contains(exception.Errors, e => e.Message == "Please enter notes of less than 255 characters");
        }

        [Fact]
        public async Task UpdateProjectAsync_PreparedByMissing_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project { ProjectId = projectId };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Valid Title",
                PreparedBy = "",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1
            };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.UpdateProjectAsync(projectId, projectDto));
            Assert.Single(exception.Errors);
            Assert.Contains(exception.Errors, e => e.Message == "Please enter who has prepared this");
        }

        [Fact]
        public async Task UpdateProjectAsync_InflationChanged_TriggersRecost()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project
            {
                ProjectId = projectId,
                Inflation = 1,
                IsDefraProject = 0
            };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Updated Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 0,
                Inflation = 2 // Changed inflation
            };

            var resultProject = new Project { ProjectId = projectId };
            var resultDto = new ProjectDto { ProjectId = projectId };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);
            _mockMapper.When(x => x.Map(projectDto, existingProject))
                .Do(x => existingProject.Inflation = projectDto.Inflation);
            _mockRepository.UpdateProjectAsync(existingProject).Returns(resultProject);
            _mockRepository.RecostProjectAsync(projectId).Returns(true);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            await _projectService.UpdateProjectAsync(projectId, projectDto);

            // Assert - RecostProjectAsync should be called
            await _mockRepository.Received(1).UpdateProjectAsync(existingProject);
            await _mockRepository.Received(1).RecostProjectAsync(projectId);
        }

        [Fact]
        public async Task UpdateProjectAsync_IsDefraProjectChanged_TriggersRecost()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project
            {
                ProjectId = projectId,
                Inflation = 1,
                IsDefraProject = 0
            };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Updated Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1, // Changed defra project flag
                Inflation = 1
            };

            var resultProject = new Project { ProjectId = projectId };
            var resultDto = new ProjectDto { ProjectId = projectId };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);
            _mockMapper.When(x => x.Map(projectDto, existingProject))
                .Do(x => existingProject.IsDefraProject = projectDto.IsDefraProject);
            _mockRepository.UpdateProjectAsync(existingProject).Returns(resultProject);
            _mockRepository.RecostProjectAsync(projectId).Returns(true);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            await _projectService.UpdateProjectAsync(projectId, projectDto);

            // Assert - RecostProjectAsync should be called
            await _mockRepository.Received(1).UpdateProjectAsync(existingProject);
            await _mockRepository.Received(1).RecostProjectAsync(projectId);
        }

        [Fact]
        public async Task UpdateProjectAsync_RecostThrowsException_SwallowsExceptionAndReturnsResult()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project
            {
                ProjectId = projectId,
                Inflation = 1,
                IsDefraProject = 0
            };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Updated Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1, // Changed defra project flag
                Inflation = 2 // Changed inflation
            };

            var resultProject = new Project { ProjectId = projectId };
            var resultDto = new ProjectDto { ProjectId = projectId };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);
            _mockMapper.When(x => x.Map(projectDto, existingProject))
                .Do(x =>
                {
                    existingProject.Inflation = projectDto.Inflation;
                    existingProject.IsDefraProject = projectDto.IsDefraProject;
                });
            _mockRepository.UpdateProjectAsync(existingProject).Returns(resultProject);
            _mockRepository.RecostProjectAsync(projectId).Returns(Task.FromException<bool>(new Exception("Recost failed")));
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.UpdateProjectAsync(projectId, projectDto);

            // Assert - Exception should be swallowed, update should still succeed
            Assert.NotNull(result);
            await _mockRepository.Received(1).UpdateProjectAsync(existingProject);
            await _mockRepository.Received(1).RecostProjectAsync(projectId);
        }

        [Fact]
        public async Task UpdateProjectAsync_NoChangesRequiringRecost_DoesNotTriggerRecost()
        {
            // Arrange
            var projectId = "P001";
            var existingProject = new Project
            {
                ProjectId = projectId,
                Inflation = 1,
                IsDefraProject = 0
            };
            var projectDto = new ProjectDto
            {
                ProjectId = projectId,
                ProjectTitle = "Updated Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 0,
                Inflation = 1
            };

            var resultProject = new Project { ProjectId = projectId };
            var resultDto = new ProjectDto { ProjectId = projectId };

            _mockRepository.GetProjectByIdAsync(projectId).Returns(existingProject);
            _mockRepository.UpdateProjectAsync(existingProject).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            await _projectService.UpdateProjectAsync(projectId, projectDto);

            // Assert - RecostProjectAsync should not be called
            await _mockRepository.Received(1).UpdateProjectAsync(existingProject);
            await _mockRepository.DidNotReceive().RecostProjectAsync(Arg.Any<string>());
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_ValidId_ReturnsRepositoryResult()
        {
            // Arrange
            var projectId = "P001";
            _mockRepository.DeleteProjectAsync(projectId).Returns(true);

            // Act
            var result = await _projectService.DeleteProjectAsync(projectId);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteProjectAsync(projectId);
        }

        [Fact]
        public async Task DeleteProjectAsync_InvalidId_ReturnsRepositoryResult()
        {
            // Arrange
            var projectId = "P999";
            _mockRepository.DeleteProjectAsync(projectId).Returns(false);

            // Act
            var result = await _projectService.DeleteProjectAsync(projectId);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteProjectAsync(projectId);
        }

        #endregion

        #region CopyProjectAsync Tests

        [Fact]
        public async Task CopyProjectAsync_ValidOldId_WithNewId_ReturnsNewProject()
        {
            // Arrange
            var oldId = "P001";
            var newId = "P002";
            var oldProject = new Project
            {
                ProjectId = oldId,
                ProjectTitle = "Original Project",
                PreparedBy = "John Doe"
            };
            var newProjectDto = new ProjectDto
            {
                ProjectId = newId,
                ProjectTitle = "Original Project",
                PreparedBy = "John Doe"
            };
            var newProject = new Project { ProjectId = newId };
            var resultProject = new Project { ProjectId = newId };
            var resultDto = new ProjectDto { ProjectId = newId };

            _mockRepository.GetProjectByIdAsync(oldId).Returns(oldProject);
            _mockMapper.Map<ProjectDto>(oldProject).Returns(newProjectDto);
            _mockMapper.Map<Project>(newProjectDto).Returns(newProject);
            _mockRepository.CopyProjectAsync(newProject, oldId).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.CopyProjectAsync(oldId, newId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newId, newProjectDto.ProjectId);
            await _mockRepository.Received(1).GetProjectByIdAsync(oldId);
            await _mockRepository.Received(1).CopyProjectAsync(newProject, oldId);
        }

        [Fact]
        public async Task CopyProjectAsync_ValidOldId_EmptyNewId_GeneratesNewId()
        {
            // Arrange
            var oldId = "P001";
            var newId = "";
            var generatedId = "P002";
            var oldProject = new Project
            {
                ProjectId = oldId,
                ProjectTitle = "Original Project"
            };
            var newProjectDto = new ProjectDto
            {
                ProjectId = generatedId,
                ProjectTitle = "Original Project"
            };
            var newProject = new Project { ProjectId = generatedId };
            var resultProject = new Project { ProjectId = generatedId };
            var resultDto = new ProjectDto { ProjectId = generatedId };

            _mockRepository.GetProjectByIdAsync(oldId).Returns(oldProject);
            _mockRepository.GetNextProjectNumberAsync(oldId).Returns(generatedId);
            _mockMapper.Map<ProjectDto>(oldProject).Returns(newProjectDto);
            _mockMapper.Map<Project>(newProjectDto).Returns(newProject);
            _mockRepository.CopyProjectAsync(newProject, oldId).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.CopyProjectAsync(oldId, newId);

            // Assert
            Assert.Equal(generatedId, newProjectDto.ProjectId);
            await _mockRepository.Received(1).GetNextProjectNumberAsync(oldId);
        }

        [Fact]
        public async Task CopyProjectAsync_ValidOldId_NullNewId_GeneratesNewId()
        {
            // Arrange
            var oldId = "P001";
            string? newId = null;
            var generatedId = "P002";
            var oldProject = new Project
            {
                ProjectId = oldId,
                ProjectTitle = "Original Project"
            };
            var newProjectDto = new ProjectDto
            {
                ProjectId = generatedId,
                ProjectTitle = "Original Project"
            };
            var newProject = new Project { ProjectId = generatedId };
            var resultProject = new Project { ProjectId = generatedId };
            var resultDto = new ProjectDto { ProjectId = generatedId };

            _mockRepository.GetProjectByIdAsync(oldId).Returns(oldProject);
            _mockRepository.GetNextProjectNumberAsync(oldId).Returns(generatedId);
            _mockMapper.Map<ProjectDto>(oldProject).Returns(newProjectDto);
            _mockMapper.Map<Project>(newProjectDto).Returns(newProject);
            _mockRepository.CopyProjectAsync(newProject, oldId).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.CopyProjectAsync(oldId, newId!);

            // Assert
            Assert.Equal(generatedId, newProjectDto.ProjectId);
            await _mockRepository.Received(1).GetNextProjectNumberAsync(oldId);
        }

        [Fact]
        public async Task CopyProjectAsync_ProjectNotFound_ThrowsArgumentException()
        {
            // Arrange
            var oldId = "P999";
            var newId = "P002";
            _mockRepository.GetProjectByIdAsync(oldId).Returns((Project?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _projectService.CopyProjectAsync(oldId, newId));
            Assert.Equal($"Project with ID '{oldId}' not found", exception.Message);
        }

        [Fact]
        public async Task CopyProjectAsync_EmptyNewId_FailsToGenerateId_ThrowsInvalidOperationException()
        {
            // Arrange
            var oldId = "P001";
            var newId = "";
            var oldProject = new Project
            {
                ProjectId = oldId,
                ProjectTitle = "Original Project"
            };

            _mockRepository.GetProjectByIdAsync(oldId).Returns(oldProject);
            _mockRepository.GetNextProjectNumberAsync(oldId).Returns(""); // Returns empty string

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _projectService.CopyProjectAsync(oldId, newId));
            Assert.Equal("Failed to generate a new project ID", exception.Message);
        }

        #endregion

        #region RecostProjectAsync Tests

        [Fact]
        public async Task RecostProjectAsync_ValidId_ReturnsRepositoryResult()
        {
            // Arrange
            var projectId = "P001";
            _mockRepository.RecostProjectAsync(projectId).Returns(true);

            // Act
            var result = await _projectService.RecostProjectAsync(projectId);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).RecostProjectAsync(projectId);
        }

        [Fact]
        public async Task RecostProjectAsync_ReturnsFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            var projectId = "P001";
            _mockRepository.RecostProjectAsync(projectId).Returns(false);

            // Act
            var result = await _projectService.RecostProjectAsync(projectId);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).RecostProjectAsync(projectId);
        }

        #endregion

        #region GetNextProjectNumberAsync Tests

        [Fact]
        public async Task GetNextProjectNumberAsync_WithBaseNumber_ReturnsRepositoryResult()
        {
            // Arrange
            var baseNumber = "P001";
            var nextNumber = "P002";
            _mockRepository.GetNextProjectNumberAsync(baseNumber).Returns(nextNumber);

            // Act
            var result = await _projectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal(nextNumber, result);
            await _mockRepository.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_WithNullBaseNumber_ReturnsRepositoryResult()
        {
            // Arrange
            string? baseNumber = null;
            var nextNumber = "P001";
            _mockRepository.GetNextProjectNumberAsync(baseNumber).Returns(nextNumber);

            // Act
            var result = await _projectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal(nextNumber, result);
            await _mockRepository.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_WithEmptyBaseNumber_ReturnsRepositoryResult()
        {
            // Arrange
            var baseNumber = "";
            var nextNumber = "P001";
            _mockRepository.GetNextProjectNumberAsync(baseNumber).Returns(nextNumber);

            // Act
            var result = await _projectService.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal(nextNumber, result);
            await _mockRepository.Received(1).GetNextProjectNumberAsync(baseNumber);
        }

        #endregion

        #region CalculateStartFinancialYear Tests (via AddProject/UpdateProject)

        [Theory]
        [InlineData(2024, 1, 15, 0, 2024)] // Project Years - January
        [InlineData(2024, 6, 15, 0, 2024)] // Project Years - June
        [InlineData(2024, 1, 15, 1, 2023)] // Financial Years - January (previous year)
        [InlineData(2024, 2, 15, 1, 2023)] // Financial Years - February (previous year)
        [InlineData(2024, 3, 31, 1, 2023)] // Financial Years - March (previous year)
        [InlineData(2024, 4, 1, 1, 2024)]  // Financial Years - April (current year)
        [InlineData(2024, 12, 31, 1, 2024)] // Financial Years - December (current year)
        public async Task AddProjectAsync_CalculateStartFinancialYear_SetsCorrectStartYear(
            int year, int month, int day, int FinancialYears, int expectedStartYear)
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = "Test Project",
                PreparedBy = "John Doe",
                Startdate = new DateTime(year, month, day),
                FinancialYears = FinancialYears,
                IsDefraProject = 1
            };

            var project = new Project();
            var resultProject = new Project();
            var resultDto = new ProjectDto();

            _mockMapper.Map<Project>(projectDto).Returns(project);
            _mockRepository.AddProjectAsync(project).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            await _projectService.AddProjectAsync(projectDto);

            // Assert
            Assert.Equal(expectedStartYear, projectDto.StartFYear);
        }

        [Fact]
        public async Task AddProjectAsync_NoStartDate_DoesNotCalculateStartYear()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = "Test Project",
                PreparedBy = "John Doe",
                Startdate = null, // No start date
                IsDefraProject = 1
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.AddProjectAsync(projectDto));
            Assert.Contains(exception.Errors, e => e.Message == "Please enter Start Date");
        }

        #endregion

        #region Validation Tests (via AddProject/UpdateProject)

        [Fact]
        public async Task ValidateProject_AllFieldsValid_ReturnsEmptyString()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = "Valid Title",
                PreparedBy = "John Doe",
                Startdate = new DateTime(2024, 4, 1),
                IsDefraProject = 1
            };

            var project = new Project();
            var resultProject = new Project();
            var resultDto = new ProjectDto();

            _mockMapper.Map<Project>(projectDto).Returns(project);
            _mockRepository.AddProjectAsync(project).Returns(resultProject);
            _mockMapper.Map<ProjectDto>(resultProject).Returns(resultDto);

            // Act
            var result = await _projectService.AddProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ValidateProject_AllFieldsInvalid_ReturnsAllErrorMessages()
        {
            // Arrange
            var projectDto = new ProjectDto
            {
                ProjectId = "P001",
                ProjectTitle = null, // Invalid
                PreparedBy = null, // Invalid
                Startdate = null, // Invalid
                IsDefraProject = null // Invalid
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _projectService.AddProjectAsync(projectDto));

            Assert.Contains(exception.Errors, e => e.Message == "Please enter Start Date");
            Assert.Contains(exception.Errors, e => e.Message == "Please enter a title");
            Assert.Contains(exception.Errors, e => e.Message == "Please choose Defra/Non-Defra");
        }

        #endregion
    }
}