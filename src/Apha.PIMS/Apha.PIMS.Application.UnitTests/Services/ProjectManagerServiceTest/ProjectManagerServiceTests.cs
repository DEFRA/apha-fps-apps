using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ProjectManagerServiceTest
{
    public class ProjectManagerServiceTests
    {
        private readonly IProjectManagerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectManagerService _service;

        public ProjectManagerServiceTests()
        {
            _repository = Substitute.For<IProjectManagerRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new ProjectManagerService(_repository, _mapper);
        }

        private static ProjectManager MakeEntity(string name = "J. Smith", string? loginEmail = null, string? email = null, string? mNumber = null) =>
            new()
            {
                Projectmanager = name,
                LoginEmail = loginEmail,
                Email = email,
                Mnumber = mNumber,
                Disable = false
            };

        private static ProjectManagerDto MakeDto(string name = "J. Smith", string? loginEmail = null, string? email = null, string? mNumber = null) =>
            new()
            {
                ProjectManager = name,
                LoginEmail = loginEmail,
                Email = email,
                MNumber = mNumber,
                Disable = false
            };

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectManagerService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectManagerService(_repository, null!));
        }

        [Fact]
        public async Task GetProjectManagerByNameAsync_Whitespace_ReturnsNull()
        {
            var result = await _service.GetProjectManagerByNameAsync("   ");
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProjectManagerAsync_DuplicateName_ThrowsBusinessValidationErrorException()
        {
            var dto = MakeDto(" Existing ");
            _repository.GetAllProjectManagersAsync().Returns(new List<ProjectManager> { MakeEntity("existing") });

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateProjectManagerAsync(dto));

            Assert.Equal("PROJECT_MANAGER_DUPLICATE_NAME", ex.Errors[0].Code);
        }

        [Fact]
        public async Task CreateProjectManagerAsync_DuplicateLoginEmail_ThrowsBusinessValidationErrorException()
        {
            var dto = MakeDto("New", loginEmail: "dup@a.gov");
            _repository.GetAllProjectManagersAsync().Returns(new List<ProjectManager> { MakeEntity("Existing", loginEmail: "DUP@a.gov") });

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateProjectManagerAsync(dto));

            Assert.Equal("PROJECT_MANAGER_DUPLICATE_LOGIN_EMAIL", ex.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateProjectManagerAsync_NotFound_ThrowsBusinessValidationErrorException()
        {
            var dto = MakeDto("Unknown");
            _repository.GetAllProjectManagersAsync().Returns(new List<ProjectManager> { MakeEntity("Known") });

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.UpdateProjectManagerAsync(dto));

            Assert.Equal("PROJECT_MANAGER_NOT_FOUND", ex.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteProjectManagerAsync_EmptyName_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.DeleteProjectManagerAsync(""));
            Assert.Equal("PROJECT_MANAGER_NAME_REQUIRED", ex.Errors[0].Code);
        }

        [Fact]
        public async Task ProjectManagerExistsAsync_TrimmedCaseInsensitive_ReturnsTrue()
        {
            _repository.GetAllProjectManagersAsync().Returns(new List<ProjectManager> { MakeEntity("John.Smith") });

            var result = await _service.ProjectManagerExistsAsync("  john.smith  ");

            Assert.True(result);
        }
    }
}
