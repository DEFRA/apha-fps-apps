using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.JobCodeServiceTest
{
    public class JobCodeServiceTests
    {
        private readonly IJobCodeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly JobCodeService _sut;

        public JobCodeServiceTests()
        {
            _mockRepository = Substitute.For<IJobCodeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new JobCodeService(_mockRepository, _mockMapper);
        }

        #region GetJobCodesByProjectAsync

        [Fact]
        public async Task GetJobCodesByProjectAsync_ValidProject_ReturnsMappedDtos()
        {
            var entities = new List<JobCode> { new JobCode { JobCodeId = "JC1", ParentProject = "PRJ1" } };
            var dtos = new List<JobCodeDto> { new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" } };

            _mockRepository.GetJobCodesByProjectAsync("PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<JobCodeDto>>(entities).Returns(dtos);

            var result = await _sut.GetJobCodesByProjectAsync("PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetJobCodesByProjectAsync("PRJ1");
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<JobCode>();
            var dtos = new List<JobCodeDto>();

            _mockRepository.GetJobCodesByProjectAsync("PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<JobCodeDto>>(entities).Returns(dtos);

            var result = await _sut.GetJobCodesByProjectAsync("PRJ1");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetJobCodesByProjectAsync("PRJ1").Throws(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetJobCodesByProjectAsync("PRJ1"));
        }

        #endregion

        #region GetPagedJobCodesAsync

        [Fact]
        public async Task GetPagedJobCodesAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<JobCode>(new List<JobCode>(), new PaginationData());
            var pagedResult = new PaginatedResult<JobCodeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedJobCodesAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<JobCodeDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedJobCodesAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedJobCodesAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_NullParentProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<JobCode>(new List<JobCode>(), new PaginationData());
            var pagedResult = new PaginatedResult<JobCodeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedJobCodesAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<JobCodeDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedJobCodesAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedJobCodesAsync(mappedParams, null);
        }

        #endregion

        #region GetJobCodeByIdAsync

        [Fact]
        public async Task GetJobCodeByIdAsync_ValidId_ReturnsMappedDto()
        {
            var entity = new JobCode { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };

            _mockRepository.GetJobCodeByIdAsync("JC1").Returns(entity);
            _mockMapper.Map<JobCodeDto>(entity).Returns(dto);

            var result = await _sut.GetJobCodeByIdAsync("JC1");

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetJobCodeByIdAsync("JC1");
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetJobCodeByIdAsync("MISSING").Returns((JobCode?)null);

            var result = await _sut.GetJobCodeByIdAsync("MISSING");

            result.Should().BeNull();
        }

        #endregion

        #region GetTypesAsync

        [Fact]
        public async Task GetTypesAsync_ReturnsTypesFromRepository()
        {
            var types = new List<string> { "TypeA", "TypeB" };
            _mockRepository.GetTypesAsync().Returns(types);

            var result = await _sut.GetTypesAsync();

            result.Should().BeEquivalentTo(types);
            await _mockRepository.Received(1).GetTypesAsync();
        }

        #endregion

        #region CreateJobCodeAsync

        [Fact]
        public async Task CreateJobCodeAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" };
            var entity = new JobCode { JobCodeId = "JC1", ParentProject = "PRJ1" };
            var created = new JobCode { JobCodeId = "JC1", ParentProject = "PRJ1" };
            var expected = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ1" };

            _mockMapper.Map<JobCode>(dto).Returns(entity);
            _mockRepository.CreateJobCodeAsync(entity).Returns(created);
            _mockMapper.Map<JobCodeDto>(created).Returns(expected);

            var result = await _sut.CreateJobCodeAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<JobCode>(dto);
            await _mockRepository.Received(1).CreateJobCodeAsync(entity);
        }

        [Fact]
        public async Task CreateJobCodeAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            var entity = new JobCode { JobCodeId = "JC1" };

            _mockMapper.Map<JobCode>(dto).Returns(entity);
            _mockRepository.CreateJobCodeAsync(entity).Throws(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateJobCodeAsync(dto));
        }

        #endregion

        #region UpdateJobCodeAsync

        [Fact]
        public async Task UpdateJobCodeAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new JobCodeDto { JobCodeId = "JC1", JobCodeName = "Updated" };
            var entity = new JobCode { JobCodeId = "JC1", JobCodeName = "Updated" };
            var updated = new JobCode { JobCodeId = "JC1", JobCodeName = "Updated" };
            var expected = new JobCodeDto { JobCodeId = "JC1", JobCodeName = "Updated" };

            _mockMapper.Map<JobCode>(dto).Returns(entity);
            _mockRepository.UpdateJobCodeAsync(entity).Returns(updated);
            _mockMapper.Map<JobCodeDto>(updated).Returns(expected);

            var result = await _sut.UpdateJobCodeAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<JobCode>(dto);
            await _mockRepository.Received(1).UpdateJobCodeAsync(entity);
        }

        [Fact]
        public async Task UpdateJobCodeAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new JobCodeDto { JobCodeId = "JC1" };
            var entity = new JobCode { JobCodeId = "JC1" };

            _mockMapper.Map<JobCode>(dto).Returns(entity);
            _mockRepository.UpdateJobCodeAsync(entity).Throws(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateJobCodeAsync(dto));
        }

        #endregion

        #region DeleteJobCodeAsync

        [Fact]
        public async Task DeleteJobCodeAsync_ValidId_ReturnsTrue()
        {
            _mockRepository.DeleteJobCodeAsync("JC1").Returns(true);

            var result = await _sut.DeleteJobCodeAsync("JC1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteJobCodeAsync("JC1");
        }

        [Fact]
        public async Task DeleteJobCodeAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteJobCodeAsync("MISSING").Returns(false);

            var result = await _sut.DeleteJobCodeAsync("MISSING");

            result.Should().BeFalse();
        }

        #endregion
    }
}
