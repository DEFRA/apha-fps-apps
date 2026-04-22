using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.DivisionServiceTest
{
    public class DivisionServiceTests
    {
        private readonly IDivisionRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly DivisionService _sut;

        public DivisionServiceTests()
        {
            _mockRepository = Substitute.For<IDivisionRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new DivisionService(_mockRepository, _mockMapper);
        }

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsMappedDtos()
        {
            // Arrange
            var divisions = new List<Division>
            {
                new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };
            var dtos = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };

            _mockRepository.GetAllDivisionsAsync().Returns(divisions);
            _mockMapper.Map<List<DivisionDto>>(divisions).Returns(dtos);

            // Act
            var result = await _sut.GetAllDivisionsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllDivisionsAsync();
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsEmptyList_WhenNoDivisions()
        {
            // Arrange
            var divisions = new List<Division>();
            var dtos = new List<DivisionDto>();

            _mockRepository.GetAllDivisionsAsync().Returns(divisions);
            _mockMapper.Map<List<DivisionDto>>(divisions).Returns(dtos);

            // Act
            var result = await _sut.GetAllDivisionsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetAllDivisionsPagedAsync Tests

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<Division>
            {
                Data = new List<Division>
                {
                    new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
                },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var pagedResult = new PaginatedResult<DivisionDto>
            {
                Data = new List<DivisionDto>
                {
                    new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
                },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllDivisionsPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<DivisionDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetAllDivisionsPagedAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllDivisionsPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _sut.GetAllDivisionsPagedAsync(null!));
        }

        #endregion

        #region GetDivisionByNameAsync Tests

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsMappedDto_WhenDivisionExists()
        {
            // Arrange
            var divName = "VSD";
            var division = new Division { DivName = divName, DivisionId = 1, AgencyId = 1 };
            var dto = new DivisionDto { DivName = divName, DivisionId = 1, AgencyId = 1 };

            _mockRepository.GetDivisionByNameAsync(divName).Returns(division);
            _mockMapper.Map<DivisionDto>(division).Returns(dto);

            // Act
            var result = await _sut.GetDivisionByNameAsync(divName);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetDivisionByNameAsync(divName);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsNull_WhenDivisionNotFound()
        {
            // Arrange
            _mockRepository.GetDivisionByNameAsync("NONEXISTENT").Returns((Division?)null);

            // Act
            var result = await _sut.GetDivisionByNameAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ThrowsArgumentException_WhenDivNameIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.GetDivisionByNameAsync(""));
        }

        #endregion

        #region CreateDivisionAsync Tests

        [Fact]
        public async Task CreateDivisionAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto = new DivisionDto { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var entity = new Division { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var created = new Division { DivName = "NEW", DivisionId = 99, AgencyId = 1 };

            _mockMapper.Map<Division>(dto).Returns(entity);
            _mockRepository.GetDivisionForeignKeyReferencesAsync("NEW").Returns(new List<string>());
            _mockRepository.DivisionExistsAsync("NEW").Returns(false);
            _mockRepository.CreateDivisionAsync(entity).Returns(created);
            _mockMapper.Map<DivisionDto>(created).Returns(dto);

            // Act
            var result = await _sut.CreateDivisionAsync(dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).CreateDivisionAsync(entity);
        }

        [Fact]
        public async Task CreateDivisionAsync_ThrowsInvalidOperationException_WhenFKReferencesExist()
        {
            // Arrange
            var dto = new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var fkTables = new List<string> { "tblkpprofitcentre" };

            _mockRepository.GetDivisionForeignKeyReferencesAsync("VSD").Returns(fkTables);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.CreateDivisionAsync(dto));
            
            exception.Message.Should().Be("Unable to add the division name as it is already in use.");
        }

        [Fact]
        public async Task CreateDivisionAsync_ThrowsInvalidOperationException_WhenDivisionAlreadyExists()
        {
            // Arrange
            var dto = new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 };

            _mockRepository.GetDivisionForeignKeyReferencesAsync("VSD").Returns(new List<string>());
            _mockRepository.DivisionExistsAsync("VSD").Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.CreateDivisionAsync(dto));
            
            exception.Message.Should().Be("Division 'VSD' already exists.");
        }

        [Fact]
        public async Task CreateDivisionAsync_ThrowsArgumentException_WhenDivNameIsEmpty()
        {
            // Arrange
            var dto = new DivisionDto { DivName = "", DivisionId = 1, AgencyId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.CreateDivisionAsync(dto));
        }

        [Fact]
        public async Task CreateDivisionAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _sut.CreateDivisionAsync(null!));
        }

        #endregion

        #region UpdateDivisionAsync Tests

        [Fact]
        public async Task UpdateDivisionAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var originalDivName = "VSD";
            var dto = new DivisionDto { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var entity = new Division { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var existing = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var updated = new Division { DivName = "VSD", DivisionId = 2, AgencyId = 2 };

            _mockRepository.GetDivisionByNameAsync(originalDivName).Returns(existing);
            _mockMapper.Map<Division>(dto).Returns(entity);
            _mockRepository.UpdateDivisionAsync(originalDivName, entity).Returns(updated);
            _mockMapper.Map<DivisionDto>(updated).Returns(dto);

            // Act
            var result = await _sut.UpdateDivisionAsync(originalDivName, dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).UpdateDivisionAsync(originalDivName, entity);
        }

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsInvalidOperationException_WhenDivisionNotFound()
        {
            // Arrange
            var dto = new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            _mockRepository.GetDivisionByNameAsync("NONEXISTENT").Returns((Division?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.UpdateDivisionAsync("NONEXISTENT", dto));
            
            exception.Message.Should().Be("Division 'NONEXISTENT' not found.");
        }

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsInvalidOperationException_WhenRenamingAndNewNameExists()
        {
            // Arrange
            var originalDivName = "VSD";
            var dto = new DivisionDto { DivName = "NEWNAME", DivisionId = 1, AgencyId = 1 };
            var existing = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 };

            _mockRepository.GetDivisionByNameAsync(originalDivName).Returns(existing);
            _mockRepository.DivisionExistsAsync("NEWNAME").Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.UpdateDivisionAsync(originalDivName, dto));
            
            exception.Message.Should().Be("Cannot rename to 'NEWNAME' - division already exists.");
        }

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsInvalidOperationException_WhenRenamingAndFKReferencesExist()
        {
            // Arrange
            var originalDivName = "VSD";
            var dto = new DivisionDto { DivName = "NEWNAME", DivisionId = 1, AgencyId = 1 };
            var existing = new Division { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var fkTables = new List<string> { "tblkpprofitcentre" };

            _mockRepository.GetDivisionByNameAsync(originalDivName).Returns(existing);
            _mockRepository.DivisionExistsAsync("NEWNAME").Returns(false);
            _mockRepository.GetDivisionForeignKeyReferencesAsync(originalDivName).Returns(fkTables);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.UpdateDivisionAsync(originalDivName, dto));
            
            exception.Message.Should().Be("Unable to edit the division name as it is already in use.");
        }

        #endregion

        #region DeleteDivisionAsync Tests

        [Fact]
        public async Task DeleteDivisionAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var divName = "VSD";
            _mockRepository.GetDivisionForeignKeyReferencesAsync(divName).Returns(new List<string>());
            _mockRepository.DeleteDivisionAsync(divName).Returns(true);

            // Act
            var result = await _sut.DeleteDivisionAsync(divName);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteDivisionAsync(divName);
        }

        [Fact]
        public async Task DeleteDivisionAsync_ThrowsInvalidOperationException_WhenFKReferencesExist()
        {
            // Arrange
            var divName = "VSD";
            var fkTables = new List<string> { "tblkpprofitcentre", "divisiongrade" };

            _mockRepository.GetDivisionForeignKeyReferencesAsync(divName).Returns(fkTables);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.DeleteDivisionAsync(divName));
            
            exception.Message.Should().Be("Unable to delete the division name as it is already in use.");
        }

        [Fact]
        public async Task DeleteDivisionAsync_ThrowsArgumentException_WhenDivNameIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.DeleteDivisionAsync(""));
        }

        #endregion
    }
}
