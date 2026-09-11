using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.MasterLookupServiceTest
{
    public class MasterLookupServiceTests
    {
        private const string TableName = "directorate";

        private readonly IMasterLookupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly MasterLookupService _sut;

        public MasterLookupServiceTests()
        {
            _mockRepository = Substitute.For<IMasterLookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new MasterLookupService(_mockRepository, _mockMapper);
        }

        #region GetAllMasterLookupsAsync

        [Fact]
        public async Task GetAllMasterLookupsAsync_ReturnsTableNames()
        {
            // Arrange
            var lookups = new List<MasterLookup>
            {
                new() { MasterTableName = "directorate" },
                new() { MasterTableName = "disease" }
            };
            _mockRepository.GetAllMasterLookupsAsync().Returns(lookups);

            // Act
            var result = await _sut.GetAllMasterLookupsAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("directorate", list[0]);
            Assert.Equal("disease", list[1]);
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_EmptyRepository_ReturnsEmpty()
        {
            // Arrange
            _mockRepository.GetAllMasterLookupsAsync().Returns(new List<MasterLookup>());

            // Act
            var result = await _sut.GetAllMasterLookupsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetLookupItemsAsync

        [Fact]
        public async Task GetLookupItemsAsync_DelegatesToRepository()
        {
            // Arrange
            var items = new List<string> { "A", "B" };
            _mockRepository.GetLookupItemsAsync(TableName).Returns(items);

            // Act
            var result = await _sut.GetLookupItemsAsync(TableName);

            // Assert
            Assert.Equal(items, result);
            await _mockRepository.Received(1).GetLookupItemsAsync(TableName);
        }

        #endregion

        #region GetLookupItemsPagedAsync

        [Fact]
        public async Task GetLookupItemsPagedAsync_MapsAndDelegates()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<string>(new List<string> { "A" }, new PaginationData { TotalRecords = 1 });
            var expected = new PaginatedResult<string>(new List<string> { "A" }, new PaginationDto { TotalRecords = 1 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetLookupItemsPagedAsync(TableName, mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<string>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            Assert.Same(expected, result);
            await _mockRepository.Received(1).GetLookupItemsPagedAsync(TableName, mappedParams);
        }

        #endregion

        #region CreateLookupItemAsync

        [Fact]
        public async Task CreateLookupItemAsync_ValidUniqueValue_ReturnsTrue()
        {
            // Arrange
            _mockRepository.GetLookupItemsAsync(TableName).Returns(new List<string> { "Existing" });
            _mockRepository.CreateLookupItemAsync(TableName, "New").Returns(true);

            // Act
            var result = await _sut.CreateLookupItemAsync(TableName, "New");

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).CreateLookupItemAsync(TableName, "New");
        }

        [Fact]
        public async Task CreateLookupItemAsync_EmptyTableName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.CreateLookupItemAsync(" ", "New"));
        }

        [Fact]
        public async Task CreateLookupItemAsync_EmptyValue_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.CreateLookupItemAsync(TableName, " "));
        }

        [Fact]
        public async Task CreateLookupItemAsync_DuplicateValue_ThrowsBusinessValidationError()
        {
            // Arrange
            _mockRepository.GetLookupItemsAsync(TableName).Returns(new List<string> { "Existing" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.CreateLookupItemAsync(TableName, "existing"));
            Assert.Contains(ex.Errors, e => e.Code == "LOOKUP_DUPLICATE_VALUE");
            await _mockRepository.DidNotReceive().CreateLookupItemAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region UpdateLookupItemAsync

        [Fact]
        public async Task UpdateLookupItemAsync_ValidValues_ReturnsTrue()
        {
            // Arrange
            _mockRepository.UpdateLookupItemAsync(TableName, "Old", "New").Returns(true);

            // Act
            var result = await _sut.UpdateLookupItemAsync(TableName, "Old", "New");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateLookupItemAsync_EmptyTableName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.UpdateLookupItemAsync(" ", "Old", "New"));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_EmptyOriginalValue_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.UpdateLookupItemAsync(TableName, " ", "New"));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_EmptyNewValue_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.UpdateLookupItemAsync(TableName, "Old", " "));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_ForeignKeyViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            _mockRepository.UpdateLookupItemAsync(TableName, "Old", "New").Throws(BuildFkViolation());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.UpdateLookupItemAsync(TableName, "Old", "New"));
            Assert.Contains(ex.Errors, e => e.Code == "LOOKUP_FK_VIOLATION");
        }

        [Fact]
        public async Task UpdateLookupItemAsync_UnexpectedError_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.UpdateLookupItemAsync(TableName, "Old", "New").Throws(new Exception("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.UpdateLookupItemAsync(TableName, "Old", "New"));
        }

        #endregion

        #region DeleteLookupItemAsync

        [Fact]
        public async Task DeleteLookupItemAsync_ValidValue_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteLookupItemAsync(TableName, "A").Returns(true);

            // Act
            var result = await _sut.DeleteLookupItemAsync(TableName, "A");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_EmptyValue_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.DeleteLookupItemAsync(TableName, " "));
        }

        [Fact]
        public async Task DeleteLookupItemAsync_ForeignKeyViolation_ThrowsBusinessValidationError()
        {
            // Arrange
            _mockRepository.DeleteLookupItemAsync(TableName, "A").Throws(BuildFkViolation());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.DeleteLookupItemAsync(TableName, "A"));
            Assert.Contains(ex.Errors, e => e.Code == "LOOKUP_FK_VIOLATION");
        }

        [Fact]
        public async Task DeleteLookupItemAsync_UnexpectedError_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.DeleteLookupItemAsync(TableName, "A").Throws(new Exception("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteLookupItemAsync(TableName, "A"));
        }

        #endregion

        // Builds a PostgresException carrying a foreign-key violation (SqlState 23503),
        // mimicking how Npgsql surfaces DB FK violations.
        private static PostgresException BuildFkViolation() =>
            new(
                messageText: "foreign key violation",
                severity: "ERROR",
                invariantSeverity: "ERROR",
                sqlState: PostgresErrorCodes.ForeignKeyViolation,
                detail: null,
                hint: null,
                position: 0,
                internalPosition: 0,
                internalQuery: null,
                where: null,
                schemaName: null,
                tableName: null,
                columnName: null,
                dataTypeName: null,
                constraintName: "fk_constraint");
    }
}
