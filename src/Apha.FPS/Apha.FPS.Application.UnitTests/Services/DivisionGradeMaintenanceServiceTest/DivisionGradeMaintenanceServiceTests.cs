using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.DivisionGradeMaintenanceServiceTest
{
    public class DivisionGradeMaintenanceServiceTests
    {
        private readonly IDivisionGradeMaintenanceRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly DivisionGradeMaintenanceService _sut;

        public DivisionGradeMaintenanceServiceTests()
        {
            _mockRepository = Substitute.For<IDivisionGradeMaintenanceRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new DivisionGradeMaintenanceService(_mockRepository, _mockMapper);
        }

        private static DivisionGradeMaintenanceDto BuildDto(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m };

        private static DivisionGradeMaintenance BuildEntity(string code = "A-VSD") =>
            new() { DivisionGradeCode = code, GradeCode = "A", Division = "VSD", ChargeRate = 100m, FpsYear = 2025 };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceService(null!, _mockMapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionGradeMaintenanceService(_mockRepository, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<DivisionGradeMaintenance>
            {
                Data = new List<DivisionGradeMaintenance> { BuildEntity() },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var pagedResult = new PaginatedResult<DivisionGradeMaintenanceDto>
            {
                Data = new List<DivisionGradeMaintenanceDto> { BuildDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<DivisionGradeMaintenanceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsEmptyResult_WhenNoData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<DivisionGradeMaintenance>
            {
                Data = [],
                PaginationData = new PaginationData { TotalRecords = 0 }
            };
            var emptyResult = new PaginatedResult<DivisionGradeMaintenanceDto>
            {
                Data = [],
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<DivisionGradeMaintenanceDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenCodeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync(""));
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenCodeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync("   "));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository.GetByIdAsync("NOTEXIST").Returns((DivisionGradeMaintenance?)null);

            var result = await _sut.GetByIdAsync("NOTEXIST");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedDto_WhenFound()
        {
            // Arrange
            var entity = BuildEntity("A-VSD");
            var dto = BuildDto("A-VSD");

            _mockRepository.GetByIdAsync("A-VSD").Returns(entity);
            _mockMapper.Map<DivisionGradeMaintenanceDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync("A-VSD");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync("A-VSD");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var entity = BuildEntity("A-VSD");
            var created = BuildEntity("A-VSD");

            _mockMapper.Map<DivisionGradeMaintenance>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<DivisionGradeMaintenanceDto>(created).Returns(dto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenDuplicateCode()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var entity = BuildEntity("A-VSD");

            _mockMapper.Map<DivisionGradeMaintenance>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity)
                .ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync("", BuildDto()));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync("   ", BuildDto()));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync("A-VSD", null!));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var entity = BuildEntity("A-VSD");
            var updated = BuildEntity("A-VSD");

            _mockMapper.Map<DivisionGradeMaintenance>(dto).Returns(entity);
            _mockRepository.UpdateAsync("A-VSD", entity).Returns(updated);
            _mockMapper.Map<DivisionGradeMaintenanceDto>(updated).Returns(dto);

            // Act
            var result = await _sut.UpdateAsync("A-VSD", dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).UpdateAsync("A-VSD", entity);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenCodeNotFound()
        {
            // Arrange
            var dto = BuildDto("A-VSD");
            var entity = BuildEntity("A-VSD");

            _mockMapper.Map<DivisionGradeMaintenance>(dto).Returns(entity);
            _mockRepository.UpdateAsync("NOTEXIST", entity)
                .ThrowsAsync(new InvalidOperationException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync("NOTEXIST", dto));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentException_WhenCodeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAsync(""));
        }

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentException_WhenCodeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAsync("   "));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
        {
            _mockRepository.DeleteAsync("A-VSD").Returns(true);

            var result = await _sut.DeleteAsync("A-VSD");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync("A-VSD");
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepository.DeleteAsync("NOTEXIST").Returns(false);

            var result = await _sut.DeleteAsync("NOTEXIST");

            result.Should().BeFalse();
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsGradeCodes()
        {
            // Arrange
            var gradeCodes = new List<string> { "A", "B", "C" };
            _mockRepository.GetAllGradeCodesAsync().Returns(gradeCodes);

            // Act
            var result = await _sut.GetAllGradeCodesAsync();

            // Assert
            result.Should().BeEquivalentTo(gradeCodes);
            await _mockRepository.Received(1).GetAllGradeCodesAsync();
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsEmpty_WhenNoGrades()
        {
            _mockRepository.GetAllGradeCodesAsync().Returns([]);

            var result = await _sut.GetAllGradeCodesAsync();

            result.Should().BeEmpty();
        }

        #endregion
    }
}
