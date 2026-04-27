using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.ProjectSubContractServiceTest
{
    public class ProjectSubContractServiceTests
    {
        private readonly IProjectSubContractRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectSubContractService _sut;

        public ProjectSubContractServiceTests()
        {
            _mockRepository = Substitute.For<IProjectSubContractRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectSubContractService(_mockRepository, _mockMapper);
        }

        #region GetPagedProjectSubContractsAsync

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectSubContractsAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectSubContractsAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectSubContractsAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_NullProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectSubContractsAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectSubContractsAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectSubContractsAsync(mappedParams, null);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_ValidProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync("PRJ1").Returns(2500.00m);

            var result = await _sut.GetTotalAmountAsync("PRJ1");

            result.Should().Be(2500.00m);
            await _mockRepository.Received(1).GetTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync(null).Returns(0m);

            var result = await _sut.GetTotalAmountAsync(null);

            result.Should().Be(0m);
            await _mockRepository.Received(1).GetTotalAmountAsync(null);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsMappedDto()
        {
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1" };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };

            _mockRepository.GetByIdAsync(1).Returns(entity);
            _mockMapper.Map<ProjectSubContractDto>(entity).Returns(dto);

            var result = await _sut.GetByIdAsync(1);

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetByIdAsync(99).Returns((ProjectSubContract?)null);

            var result = await _sut.GetByIdAsync(99);

            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var entity = new ProjectSubContract { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var created = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 500m };
            var expected = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 500m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<ProjectSubContractDto>(created).Returns(expected);

            var result = await _sut.CreateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectSubContract>(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "", Month = 1.0, Amount = 500m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = null, Amount = 500m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var entity = new ProjectSubContract { Project = "PRJ1", Month = 1.0, Amount = 500m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var updated = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var expected = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(updated);
            _mockMapper.Map<ProjectSubContractDto>(updated).Returns(expected);

            var result = await _sut.UpdateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectSubContract>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "", Month = 1.0, Amount = 750m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = null, Amount = 750m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
        }

        #endregion

        #region GetAnimalSubContractsAsync

        [Fact]
        public async Task GetAnimalSubContractsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<ProjectSubContract> { new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals" } };
            var pagedData = new PagedData<ProjectSubContract>(entities, new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalSubContractsAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetAnimalSubContractsAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetAnimalSubContractsAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetAnimalSubContractsAsync_NullProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalSubContractsAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetAnimalSubContractsAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetAnimalSubContractsAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetAnimalSubContractsAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalSubContractsAsync(mappedParams, "PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAnimalSubContractsAsync(query, "PRJ1"));
        }

        #endregion

        #region GetAnimalTotalAmountAsync

        [Fact]
        public async Task GetAnimalTotalAmountAsync_ValidProject_ReturnsTotalAmount()
        {
            _mockRepository.GetAnimalTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _sut.GetAnimalTotalAmountAsync("PRJ1");

            result.Should().Be(1500.00m);
            await _mockRepository.Received(1).GetAnimalTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetAnimalTotalAmountAsync_NullProject_ReturnsTotalAmount()
        {
            _mockRepository.GetAnimalTotalAmountAsync(null).Returns(0m);

            var result = await _sut.GetAnimalTotalAmountAsync(null);

            result.Should().Be(0m);
            await _mockRepository.Received(1).GetAnimalTotalAmountAsync(null);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            _mockRepository.DeleteAsync(1).Returns(true);

            var result = await _sut.DeleteAsync(1);

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteAsync(99).Returns(false);

            var result = await _sut.DeleteAsync(99);

            result.Should().BeFalse();
        }

        #endregion
    }
}
