using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.ProjectMonthServiceTest
{
    public class ProjectMonthServiceTests
    {
        private readonly IProjectMonthRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectMonthService _sut;

        public ProjectMonthServiceTests()
        {
            _mockRepository = Substitute.For<IProjectMonthRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectMonthService(_mockRepository, _mockMapper);
        }

        #region GetMonthsAsync

        [Fact]
        public async Task GetMonthsAsync_WithData_ReturnsMappedDtoList()
        {
            var entities = new List<Month>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            var dtos = new List<MonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };

            _mockRepository.GetMonthsAsync().Returns(entities);
            _mockMapper.Map<IList<MonthDto>>(entities).Returns(dtos);

            var result = await _sut.GetMonthsAsync();

            result.Should().BeSameAs(dtos);
            await _mockRepository.Received(1).GetMonthsAsync();
            _mockMapper.Received(1).Map<IList<MonthDto>>(entities);
        }

        [Fact]
        public async Task GetMonthsAsync_EmptyList_ReturnsMappedEmptyList()
        {
            var entities = new List<Month>();
            var dtos = new List<MonthDto>();

            _mockRepository.GetMonthsAsync().Returns(entities);
            _mockMapper.Map<IList<MonthDto>>(entities).Returns(dtos);

            var result = await _sut.GetMonthsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMonthsAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetMonthsAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetMonthsAsync());
        }

        #endregion

        #region GetProjectMonthByProjectAsync

        [Fact]
        public async Task GetProjectMonthByProjectAsync_ValidProject_ReturnsMappedDtoList()
        {
            var entities = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1 },
                new() { Project = "PRJ1", MonthNo = 2 }
            };
            var dtos = new List<ProjectMonthDto>
            {
                new() { Project = "PRJ1", MonthNo = 1 },
                new() { Project = "PRJ1", MonthNo = 2 }
            };

            _mockRepository.GetProjectMonthByProjectAsync("PRJ1").Returns(entities);
            _mockMapper.Map<IList<ProjectMonthDto>>(entities).Returns(dtos);

            var result = await _sut.GetProjectMonthByProjectAsync("PRJ1");

            result.Should().BeSameAs(dtos);
            await _mockRepository.Received(1).GetProjectMonthByProjectAsync("PRJ1");
            _mockMapper.Received(1).Map<IList<ProjectMonthDto>>(entities);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_NoMatchingProject_ReturnsMappedEmptyList()
        {
            var entities = new List<ProjectMonth>();
            var dtos = new List<ProjectMonthDto>();

            _mockRepository.GetProjectMonthByProjectAsync("PRJ_NONE").Returns(entities);
            _mockMapper.Map<IList<ProjectMonthDto>>(entities).Returns(dtos);

            var result = await _sut.GetProjectMonthByProjectAsync("PRJ_NONE");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetProjectMonthByProjectAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetProjectMonthByProjectAsync("PRJ1"));
        }

        #endregion

        #region GetProjectMonthAsync

        [Fact]
        public async Task GetProjectMonthAsync_ExistingRecord_ReturnsMappedDto()
        {
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 3, CostProfile = 250m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 3, CostProfile = 250m };

            _mockRepository.GetProjectMonthAsync("PRJ1", 3).Returns(entity);
            _mockMapper.Map<ProjectMonthDto>(entity).Returns(dto);

            var result = await _sut.GetProjectMonthAsync("PRJ1", 3);

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetProjectMonthAsync("PRJ1", 3);
            _mockMapper.Received(1).Map<ProjectMonthDto>(entity);
        }

        [Fact]
        public async Task GetProjectMonthAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetProjectMonthAsync("PRJ_NONE", 99).Returns((ProjectMonth?)null);

            var result = await _sut.GetProjectMonthAsync("PRJ_NONE", 99);

            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<ProjectMonthDto>(Arg.Any<ProjectMonth>());
        }

        [Fact]
        public async Task GetProjectMonthAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetProjectMonthAsync("PRJ1", 1).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetProjectMonthAsync("PRJ1", 1));
        }

        #endregion

        #region CreateProjectMonthAsync

        [Fact]
        public async Task CreateProjectMonthAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var created = new ProjectMonth { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var expected = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };

            _mockMapper.Map<ProjectMonth>(dto).Returns(entity);
            _mockRepository.CreateProjectMonthAsync(entity).Returns(created);
            _mockMapper.Map<ProjectMonthDto>(created).Returns(expected);

            var result = await _sut.CreateProjectMonthAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectMonth>(dto);
            await _mockRepository.Received(1).CreateProjectMonthAsync(entity);
            _mockMapper.Received(1).Map<ProjectMonthDto>(created);
        }

        [Fact]
        public async Task CreateProjectMonthAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "", MonthNo = 1 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
            await _mockRepository.DidNotReceive().CreateProjectMonthAsync(Arg.Any<ProjectMonth>());
        }

        [Fact]
        public async Task CreateProjectMonthAsync_WhitespaceProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "   ", MonthNo = 1 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task CreateProjectMonthAsync_ZeroMonthNo_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 0 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTHNO_REQUIRED");
        }

        [Fact]
        public async Task CreateProjectMonthAsync_NegativeMonthNo_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = -1 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTHNO_REQUIRED");
        }

        [Fact]
        public async Task CreateProjectMonthAsync_BothProjectAndMonthNoInvalid_ThrowsWithBothErrors()
        {
            var dto = new ProjectMonthDto { Project = "", MonthNo = 0 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateProjectMonthAsync(dto));

            ex.Errors.Should().HaveCount(2);
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "MONTHNO_REQUIRED");
        }

        [Fact]
        public async Task CreateProjectMonthAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1 };
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 1 };

            _mockMapper.Map<ProjectMonth>(dto).Returns(entity);
            _mockRepository.CreateProjectMonthAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateProjectMonthAsync(dto));
        }

        #endregion

        #region UpdateProjectMonthAsync

        [Fact]
        public async Task UpdateProjectMonthAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var updated = new ProjectMonth { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var expected = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };

            _mockMapper.Map<ProjectMonth>(dto).Returns(entity);
            _mockRepository.UpdateProjectMonthAsync(entity).Returns(updated);
            _mockMapper.Map<ProjectMonthDto>(updated).Returns(expected);

            var result = await _sut.UpdateProjectMonthAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectMonth>(dto);
            await _mockRepository.Received(1).UpdateProjectMonthAsync(entity);
            _mockMapper.Received(1).Map<ProjectMonthDto>(updated);
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "", MonthNo = 2 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
            await _mockRepository.DidNotReceive().UpdateProjectMonthAsync(Arg.Any<ProjectMonth>());
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_WhitespaceProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "   ", MonthNo = 2 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_ZeroMonthNo_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 0 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTHNO_REQUIRED");
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_NegativeMonthNo_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = -5 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateProjectMonthAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTHNO_REQUIRED");
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_BothProjectAndMonthNoInvalid_ThrowsWithBothErrors()
        {
            var dto = new ProjectMonthDto { Project = "", MonthNo = 0 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateProjectMonthAsync(dto));

            ex.Errors.Should().HaveCount(2);
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "MONTHNO_REQUIRED");
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2 };
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 2 };

            _mockMapper.Map<ProjectMonth>(dto).Returns(entity);
            _mockRepository.UpdateProjectMonthAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateProjectMonthAsync(dto));
        }

        #endregion

        #region DeleteProjectMonthAsync

        [Fact]
        public async Task DeleteProjectMonthAsync_ExistingRecord_ReturnsTrue()
        {
            _mockRepository.DeleteProjectMonthAsync("PRJ1", 1).Returns(true);

            var result = await _sut.DeleteProjectMonthAsync("PRJ1", 1);

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteProjectMonthAsync("PRJ1", 1);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteProjectMonthAsync("PRJ_NONE", 99).Returns(false);

            var result = await _sut.DeleteProjectMonthAsync("PRJ_NONE", 99);

            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteProjectMonthAsync("PRJ_NONE", 99);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.DeleteProjectMonthAsync("PRJ1", 1).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteProjectMonthAsync("PRJ1", 1));
        }

        #endregion
    }
}
