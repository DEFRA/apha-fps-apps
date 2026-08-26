using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.RadTrackProgServiceTest
{
    public class RadTrackProgServiceTests
    {
        private readonly IRadTrackProgRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly RadTrackProgService _sut;

        public RadTrackProgServiceTests()
        {
            _mockRepository = Substitute.For<IRadTrackProgRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new RadTrackProgService(_mockRepository, _mockMapper);
        }

        // ────── shared factory helpers ──────────────────────────────────────────────────────────

        /// <summary>Returns a <see cref="RadTrackProgDto"/> that passes all Create validation.</summary>
        private static RadTrackProgDto ValidCreateDto(string program = "TEST001") => new()
        {
            Program = program,
            RadTrackProg = true,
            PublicationPrefix = "TP"
        };

        /// <summary>Returns a <see cref="RadtrackProg"/> entity for mocking repository responses.</summary>
        private static RadtrackProg ValidEntity(string program = "TEST001") => new()
        {
            Program = program,
            Radtrackprog = true,
            Publicationprefix = "TP"
        };

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var act = () => new RadTrackProgService(null!, _mockMapper);
            act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var act = () => new RadTrackProgService(_mockRepository, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        #endregion

        #region GetAllRadTrackProgsAsync Tests

        [Fact]
        public async Task GetAllRadTrackProgsAsync_WithValidData_ReturnsListOfDtos()
        {
            // Arrange
            var entities = new List<RadtrackProg> { ValidEntity() };
            var dtos = new List<RadTrackProgDto> { ValidCreateDto() };

            _mockRepository.GetAllRadTrackProgsAsync().Returns(entities);
            _mockMapper.Map<List<RadTrackProgDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAllRadTrackProgsAsync();

            // Assert
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllRadTrackProgsAsync();
            _mockMapper.Received(1).Map<List<RadTrackProgDto>>(entities);
        }

        [Fact]
        public async Task GetAllRadTrackProgsAsync_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<RadtrackProg>();
            var dtos = new List<RadTrackProgDto>();

            _mockRepository.GetAllRadTrackProgsAsync().Returns(entities);
            _mockMapper.Map<List<RadTrackProgDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAllRadTrackProgsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetPagedRadTrackProgsAsync Tests

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_WithValidQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var paginationInfo = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<RadtrackProg>(
                new List<RadtrackProg> { ValidEntity() }.AsReadOnly(),
                paginationInfo
            );
            var expectedResult = new PaginatedResult<RadTrackProgDto>
            {
                Data = new List<RadTrackProgDto> { ValidCreateDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedRadTrackProgsAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<RadTrackProgDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedRadTrackProgsAsync(query);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        #endregion

        #region GetRadTrackProgByProgramAsync Tests

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WithValidProgram_ReturnsRadTrackProgDto()
        {
            // Arrange
            var program = "TEST001";
            var entity = ValidEntity(program);
            var dto = ValidCreateDto(program);

            _mockRepository.GetRadTrackProgByProgramAsync(program).Returns(entity);
            _mockMapper.Map<RadTrackProgDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetRadTrackProgByProgramAsync(program);

            // Assert
            result.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WithNullProgram_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _sut.GetRadTrackProgByProgramAsync(null!);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WithEmptyProgram_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _sut.GetRadTrackProgByProgramAsync("");
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WithWhitespaceProgram_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _sut.GetRadTrackProgByProgramAsync("   ");
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_WithNonExistentProgram_ReturnsNull()
        {
            // Arrange
            var program = "NONEXISTENT";
            _mockRepository.GetRadTrackProgByProgramAsync(program).Returns((RadtrackProg?)null);

            // Act
            var result = await _sut.GetRadTrackProgByProgramAsync(program);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateRadTrackProgAsync Tests

        [Fact]
        public async Task CreateRadTrackProgAsync_WithProgramLeadingTrailingSpaces_NormalizesTrimmedValue()
        {
            // Arrange - Test that spaces are trimmed
            var dto = ValidCreateDto("  TEST001  ");
            var trimmedProgram = "TEST001";
            var entity = ValidEntity(trimmedProgram);
            var createdEntity = ValidEntity(trimmedProgram);
            var createdDto = ValidCreateDto(trimmedProgram);

            _mockRepository.RadTrackProgExistsAsync(trimmedProgram).Returns(false);
            _mockMapper.Map<RadtrackProg>(Arg.Is<RadTrackProgDto>(d => d.Program == trimmedProgram)).Returns(entity);
            _mockRepository.AddRadTrackProgAsync(entity).Returns(createdEntity);
            _mockMapper.Map<RadTrackProgDto>(createdEntity).Returns(createdDto);

            // Act
            var result = await _sut.CreateRadTrackProgAsync(dto);

            // Assert - Program should be trimmed and checked
            result.Should().BeEquivalentTo(createdDto);
            await _mockRepository.Received(1).RadTrackProgExistsAsync(trimmedProgram);
            dto.Program.Should().Be(trimmedProgram); // DTO should be modified to have trimmed value
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithNullProgram_ThrowsArgumentException()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = null!, RadTrackProg = true, PublicationPrefix = "TP" };

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithEmptyProgram_ThrowsArgumentException()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = "", RadTrackProg = true, PublicationPrefix = "TP" };

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithWhitespaceProgram_ThrowsArgumentException()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = "   ", RadTrackProg = true, PublicationPrefix = "TP" };

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithDuplicateProgram_ThrowsInvalidOperationException()
        {
            // Arrange - Test exact case match
            var dto = ValidCreateDto("ADMIN");
            _mockRepository.RadTrackProgExistsAsync("ADMIN").Returns(true);

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Program 'ADMIN' already exists*");
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithDuplicateProgramLowerCase_ThrowsInvalidOperationException()
        {
            // Arrange - Test lowercase version of existing uppercase program
            // Repository will do case-insensitive comparison via ILike
            var dto = ValidCreateDto("admin");
            _mockRepository.RadTrackProgExistsAsync("admin").Returns(true); // ILike will match ADMIN too

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Program 'admin' already exists*");
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithDuplicateProgramMixedCase_ThrowsInvalidOperationException()
        {
            // Arrange - Test mixed case version of existing program
            var dto = ValidCreateDto("AdMiN");
            _mockRepository.RadTrackProgExistsAsync("AdMiN").Returns(true); // ILike will match any case variation

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Program 'AdMiN' already exists*");
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithDuplicateProgramWithSpaces_ThrowsInvalidOperationException()
        {
            // Arrange - Test program name with leading/trailing spaces that should be trimmed
            var dto = ValidCreateDto("  ADMIN  ");
            _mockRepository.RadTrackProgExistsAsync("ADMIN").Returns(true); // After trimming, should check "ADMIN"

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Program 'ADMIN' already exists*"); // Should show trimmed version in message
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_WithDuplicateProgramMultipleSpaces_ThrowsInvalidOperationException()
        {
            // Arrange - Test program name with multiple trailing spaces
            var dto = ValidCreateDto("admin    ");
            _mockRepository.RadTrackProgExistsAsync("admin").Returns(true); // After trimming, should check "admin"

            // Act & Assert
            var act = async () => await _sut.CreateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Program 'admin' already exists*");
        }

        #endregion

        #region UpdateRadTrackProgAsync Tests

        [Fact]
        public async Task UpdateRadTrackProgAsync_WithValidDto_ReturnsUpdatedDto()
        {
            // Arrange
            var dto = ValidCreateDto();
            var entity = ValidEntity();
            var updatedEntity = ValidEntity();
            var updatedDto = ValidCreateDto();

            _mockRepository.RadTrackProgExistsAsync(dto.Program).Returns(true);
            _mockMapper.Map<RadtrackProg>(dto).Returns(entity);
            _mockRepository.UpdateRadTrackProgAsync(entity).Returns(updatedEntity);
            _mockMapper.Map<RadTrackProgDto>(updatedEntity).Returns(updatedDto);

            // Act
            var result = await _sut.UpdateRadTrackProgAsync(dto);

            // Assert
            result.Should().BeEquivalentTo(updatedDto);
            await _mockRepository.Received(1).RadTrackProgExistsAsync(dto.Program);
            await _mockRepository.Received(1).UpdateRadTrackProgAsync(entity);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = async () => await _sut.UpdateRadTrackProgAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_WithNullProgram_ThrowsArgumentException()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = null!, RadTrackProg = true, PublicationPrefix = "TP" };

            // Act & Assert
            var act = async () => await _sut.UpdateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_WithNonExistentProgram_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = ValidCreateDto("NONEXISTENT");
            _mockRepository.RadTrackProgExistsAsync("NONEXISTENT").Returns(false);

            // Act & Assert
            var act = async () => await _sut.UpdateRadTrackProgAsync(dto);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*RadTrackProg with program 'NONEXISTENT' was not found*");
        }

        #endregion

        #region DeleteRadTrackProgAsync Tests

        [Fact]
        public async Task DeleteRadTrackProgAsync_WithValidProgram_ReturnsTrue()
        {
            // Arrange
            var program = "TEST001";
            _mockRepository.RadTrackProgExistsAsync(program).Returns(true);
            _mockRepository.DeleteRadTrackProgAsync(program).Returns(true);

            // Act
            var result = await _sut.DeleteRadTrackProgAsync(program);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).RadTrackProgExistsAsync(program);
            await _mockRepository.Received(1).DeleteRadTrackProgAsync(program);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_WithNullProgram_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _sut.DeleteRadTrackProgAsync(null!);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_WithEmptyProgram_ThrowsArgumentException()
        {
            // Act & Assert
            var act = async () => await _sut.DeleteRadTrackProgAsync("");
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_WithNonExistentProgram_ThrowsKeyNotFoundException()
        {
            // Arrange
            var program = "NONEXISTENT";
            _mockRepository.RadTrackProgExistsAsync(program).Returns(false);

            // Act & Assert
            var act = async () => await _sut.DeleteRadTrackProgAsync(program);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*RadTrackProg with program '{program}' was not found*");
        }

        #endregion

        #region RadTrackProgExistsAsync Tests

        [Fact]
        public async Task RadTrackProgExistsAsync_WithValidProgram_ReturnsTrue()
        {
            // Arrange
            var program = "TEST001";
            _mockRepository.RadTrackProgExistsAsync(program).Returns(true);

            // Act
            var result = await _sut.RadTrackProgExistsAsync(program);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task RadTrackProgExistsAsync_WithNonExistentProgram_ReturnsFalse()
        {
            // Arrange
            var program = "NONEXISTENT";
            _mockRepository.RadTrackProgExistsAsync(program).Returns(false);

            // Act
            var result = await _sut.RadTrackProgExistsAsync(program);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RadTrackProgExistsAsync_WithNullProgram_ReturnsFalse()
        {
            // Act
            var result = await _sut.RadTrackProgExistsAsync(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RadTrackProgExistsAsync_WithEmptyProgram_ReturnsFalse()
        {
            // Act
            var result = await _sut.RadTrackProgExistsAsync("");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetAllProgramNamesAsync Tests

        [Fact]
        public async Task GetAllProgramNamesAsync_WithValidData_ReturnsListOfProgramNames()
        {
            // Arrange
            var programNames = new List<string> { "TEST001", "TEST002", "TEST003" };
            _mockRepository.GetAllProgramNamesAsync().Returns(programNames);

            // Act
            var result = await _sut.GetAllProgramNamesAsync();

            // Assert
            result.Should().BeEquivalentTo(programNames);
            await _mockRepository.Received(1).GetAllProgramNamesAsync();
        }

        [Fact]
        public async Task GetAllProgramNamesAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            var programNames = new List<string>();
            _mockRepository.GetAllProgramNamesAsync().Returns(programNames);

            // Act
            var result = await _sut.GetAllProgramNamesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
