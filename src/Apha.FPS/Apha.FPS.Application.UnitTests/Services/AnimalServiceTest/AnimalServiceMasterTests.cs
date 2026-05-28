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

namespace Apha.FPS.Application.UnitTests.Services.AnimalServiceTest
{
    public class AnimalServiceMasterTests
    {
        private readonly IAnimalRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly AnimalService _sut;

        public AnimalServiceMasterTests()
        {
            _mockRepository = Substitute.For<IAnimalRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new AnimalService(_mockRepository, _mockMapper);
        }

        private static AnimalDto BuildDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static Animal BuildEntity(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m, FpsYear = 2025 };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimalService(null!, _mockMapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimalService(_mockRepository, null!));
        }

        #endregion

        #region GetAllAnimalsAsync (non-paged) Tests

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsMappedDtos()
        {
            var entities = new List<Animal> { BuildEntity() };
            var dtos = new List<AnimalDto> { BuildDto() };

            _mockRepository.GetAllAnimalsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<AnimalDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllAnimalsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsEmpty_WhenRepositoryReturnsEmpty()
        {
            _mockRepository.GetAllAnimalsAsync().Returns(new List<Animal>());
            _mockMapper.Map<IEnumerable<AnimalDto>>(Arg.Any<IEnumerable<Animal>>())
                .Returns(new List<AnimalDto>());

            var result = await _sut.GetAllAnimalsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ThrowsException_WhenRepositoryThrows()
        {
            _mockRepository.GetAllAnimalsAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAnimalsAsync());
        }

        #endregion

        #region GetAllAnimalsAsync (paged) Tests

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllAnimalsAsync(null!));
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PagedData<Animal>
            {
                Data = [BuildEntity()],
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginatedResult<AnimalDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllAnimalsAsync(paginationParams).Returns(paged);
            _mockMapper.Map<PaginatedResult<AnimalDto>>(paged).Returns(expected);

            var result = await _sut.GetAllAnimalsAsync(query);

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsEmpty_WhenRepositoryReturnsEmpty()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PagedData<Animal>
            {
                Data = [],
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 0, TotalRecords = 0 }
            };
            var expected = new PaginatedResult<AnimalDto>
            {
                Data = [],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 0, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllAnimalsAsync(paginationParams).Returns(paged);
            _mockMapper.Map<PaginatedResult<AnimalDto>>(paged).Returns(expected);

            var result = await _sut.GetAllAnimalsAsync(query);

            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetAnimalByIdAsync Tests

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenAnimalTypeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAnimalByIdAsync(""));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenAnimalTypeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAnimalByIdAsync("   "));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsDto_WhenFound()
        {
            var entity = BuildEntity();
            var dto = BuildDto();

            _mockRepository.GetAnimalByIdAsync("CATTLE").Returns(entity);
            _mockMapper.Map<AnimalDto?>(entity).Returns(dto);

            var result = await _sut.GetAnimalByIdAsync("CATTLE");

            result.Should().NotBeNull();
            result!.AnimalType.Should().Be("CATTLE");
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository.GetAnimalByIdAsync("NOTEXIST").Returns((Animal?)null);
            _mockMapper.Map<AnimalDto?>(null).Returns((AnimalDto?)null);

            var result = await _sut.GetAnimalByIdAsync("NOTEXIST");

            result.Should().BeNull();
        }

        #endregion

        #region AddAnimalAsync Tests

        [Fact]
        public async Task AddAnimalAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddAnimalAsync(null!));
        }

        [Fact]
        public async Task AddAnimalAsync_ThrowsArgumentException_WhenAnimalTypeIsEmpty()
        {
            var dto = new AnimalDto { AnimalType = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddAnimalAsync(dto));
        }

        [Fact]
        public async Task AddAnimalAsync_ThrowsArgumentException_WhenAnimalTypeIsWhiteSpace()
        {
            var dto = new AnimalDto { AnimalType = "   " };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddAnimalAsync(dto));
        }

        [Fact]
        public async Task AddAnimalAsync_ReturnsAddedDto()
        {
            var dto = BuildDto();
            var entity = BuildEntity();
            var addedDto = BuildDto();

            _mockMapper.Map<Animal>(dto).Returns(entity);
            _mockRepository.AddAnimalAsync(entity).Returns(entity);
            _mockMapper.Map<AnimalDto>(entity).Returns(addedDto);

            var result = await _sut.AddAnimalAsync(dto);

            result.Should().NotBeNull();
            result.AnimalType.Should().Be("CATTLE");
        }

        [Fact]
        public async Task AddAnimalAsync_ThrowsException_WhenRepositoryThrows()
        {
            var dto = BuildDto();
            _mockMapper.Map<Animal>(dto).Returns(BuildEntity());
            _mockRepository.AddAnimalAsync(Arg.Any<Animal>()).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.AddAnimalAsync(dto));
        }

        #endregion

        #region UpdateAnimalAsync Tests

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAnimalAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsArgumentException_WhenAnimalTypeIsEmpty()
        {
            var dto = new AnimalDto { AnimalType = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAnimalAsync(dto));
        }

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsKeyNotFoundException_WhenAnimalNotFound()
        {
            var dto = BuildDto("NOTEXIST");
            _mockRepository.GetAnimalByIdAsync("NOTEXIST").Returns((Animal?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAnimalAsync(dto));
        }

        [Fact]
        public async Task UpdateAnimalAsync_ReturnsUpdatedDto()
        {
            var dto = BuildDto();
            var existing = BuildEntity();
            var updatedEntity = BuildEntity();

            _mockRepository.GetAnimalByIdAsync("CATTLE").Returns(existing);
            _mockMapper.Map(dto, existing);
            _mockRepository.UpdateAnimalAsync(existing).Returns(updatedEntity);
            _mockMapper.Map<AnimalDto>(updatedEntity).Returns(dto);

            var result = await _sut.UpdateAnimalAsync(dto);

            result.Should().NotBeNull();
            result.AnimalType.Should().Be("CATTLE");
        }

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsException_WhenRepositoryThrows()
        {
            var dto = BuildDto();
            var existing = BuildEntity();
            _mockRepository.GetAnimalByIdAsync("CATTLE").Returns(existing);
            _mockMapper.Map(dto, existing);
            _mockRepository.UpdateAnimalAsync(Arg.Any<Animal>()).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAnimalAsync(dto));
        }

        #endregion

        #region DeleteAnimalAsync Tests

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAnimalAsync(""));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAnimalAsync("   "));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsTrue_WhenDeleted()
        {
            _mockRepository.DeleteAnimalAsync("CATTLE").Returns(true);

            var result = await _sut.DeleteAnimalAsync("CATTLE");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepository.DeleteAnimalAsync("NOTEXIST").Returns(false);

            var result = await _sut.DeleteAnimalAsync("NOTEXIST");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsException_WhenRepositoryThrows()
        {
            _mockRepository.DeleteAnimalAsync("CATTLE").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAnimalAsync("CATTLE"));
        }

        #endregion
    }
}
