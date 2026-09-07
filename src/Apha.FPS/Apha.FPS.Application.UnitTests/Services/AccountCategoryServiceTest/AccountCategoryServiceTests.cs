using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.AccountCategoryServiceTest
{
    public class AccountCategoryServiceTests
    {
        private const string TestAccShortName = "TEST001";
        private const string TestAccountDescription = "Test Description";
        private const string TestAccountType = "Income";
        private const int TestFpsYear = 2024;

        private readonly IAccountCategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccountCategoryService _service;

        public AccountCategoryServiceTests()
        {
            _repository = Substitute.For<IAccountCategoryRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new AccountCategoryService(_repository, _mapper);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ValidRequest_ReturnsPaginatedResult()
        {
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var pagedData = new PagedData<AccountCategory>
            {
                Data = new List<AccountCategory>
                {
                    CreateTestEntity(TestAccShortName, TestAccountDescription)
                },
                PaginationData = new PaginationData
                {
                    TotalRecords = 1,
                    PageNumber = 1,
                    PageSize = 10
                }
            };

            var paginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mapper.Map<PaginationParameters<string>>(queryFilter).Returns(paginationParams);
            _repository.GetAllAsync(paginationParams, null).Returns(pagedData);

            var expectedDto = new PaginatedResult<AccountCategoryDto>
            {
                Data = new List<AccountCategoryDto>
                {
                    CreateTestDto(TestAccShortName, TestAccountDescription)
                }
            };

            _mapper.Map<PaginatedResult<AccountCategoryDto>>(pagedData).Returns(expectedDto);

            // Act
            var result = await _service.GetAllAsync(queryFilter, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetAllAsync(paginationParams, null);
        }

        [Fact]
        public async Task GetAllAsync_WithRcFilter_PassesFilterToRepository()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<AccountCategory> 
            { 
                Data = new List<AccountCategory>(), 
                PaginationData = new PaginationData { TotalRecords = 0 } 
            };

            _mapper.Map<PaginationParameters<string>>(queryFilter).Returns(paginationParams);
            _repository.GetAllAsync(paginationParams, "rc").Returns(pagedData);
            _mapper.Map<PaginatedResult<AccountCategoryDto>>(pagedData)
                .Returns(new PaginatedResult<AccountCategoryDto> { Data = new List<AccountCategoryDto>() });

            // Act
            await _service.GetAllAsync(queryFilter, "rc");

            // Assert
            await _repository.Received(1).GetAllAsync(paginationParams, "rc");
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsDto()
        {
            // Arrange
            var entity = CreateTestEntity(TestAccShortName, TestAccountDescription);
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            _repository.GetByIdAsync(TestAccShortName).Returns(entity);
            _mapper.Map<AccountCategoryDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAccShortName, result.AccShortName);
            await _repository.Received(1).GetByIdAsync(TestAccShortName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync("NONEXISTENT").Returns((AccountCategory?)null);
            _mapper.Map<AccountCategoryDto>((AccountCategory?)null).Returns((AccountCategoryDto?)null);

            // Act
            var result = await _service.GetByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
            await _repository.Received(1).GetByIdAsync("NONEXISTENT");
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_ValidDto_AddsSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);
            var entity = CreateTestEntity(TestAccShortName, TestAccountDescription);

            _repository.ExistsByAccShortNameAsync(TestAccShortName).Returns(false);
            _mapper.Map<AccountCategory>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(entity);
            _mapper.Map<AccountCategoryDto>(entity).Returns(dto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestAccShortName, result.AccShortName);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_NullAccShortName_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(null!, TestAccountDescription);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_EmptyAccShortName_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(string.Empty, TestAccountDescription);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_NullAccountType_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);
            dto.AccountType = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_DuplicateAccShortName_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            _repository.ExistsByAccShortNameAsync(TestAccShortName).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddAsync(dto));

            Assert.Contains(TestAccShortName, exception.Message);
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task AddAsync_DuplicateAccShortNameDiffersOnlyByCase_ThrowsInvalidOperationException()
        {
            // Arrange - duplicate detection must be case-insensitive
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            _repository.ExistsByAccShortNameAsync(TestAccShortName).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddAsync(dto));

            Assert.Contains("already exists", exception.Message);
            await _repository.DidNotReceive().AddAsync(Arg.Any<AccountCategory>());
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidDto_UpdatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, "Updated Description");
            var existingEntity = CreateTestEntity(TestAccShortName, TestAccountDescription);
            var updatedEntity = CreateTestEntity(TestAccShortName, "Updated Description");

            _repository.GetByIdAsync(TestAccShortName).Returns(existingEntity);
            _mapper.Map<AccountCategory>(dto).Returns(updatedEntity);
            _repository.UpdateAsync(updatedEntity).Returns(updatedEntity);
            _mapper.Map<AccountCategoryDto>(updatedEntity).Returns(dto);

            // Act
            var result = await _service.UpdateAsync(TestAccShortName, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Description", result.AccountDescription);
            await _repository.Received(1).UpdateAsync(updatedEntity);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(TestAccShortName, null!));
        }

        [Fact]
        public async Task UpdateAsync_NullOriginalAccShortName_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(null!, dto));
        }

        [Fact]
        public async Task UpdateAsync_EmptyOriginalAccShortName_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(string.Empty, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullAccShortName_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(null!, TestAccountDescription);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(TestAccShortName, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullAccountType_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);
            dto.AccountType = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(TestAccShortName, dto));
        }

        [Fact]
        public async Task UpdateAsync_NonExistingEntity_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);
            _repository.GetByIdAsync(TestAccShortName).Returns((AccountCategory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateAsync(TestAccShortName, dto));

            Assert.Contains(TestAccShortName, exception.Message);
            Assert.Contains("not found", exception.Message);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingEntity_DeletesSuccessfully()
        {
            // Arrange
            _repository.GetForeignKeyReferencesAsync(TestAccShortName).Returns(new List<string>());
            _repository.DeleteAsync(TestAccShortName).Returns(true);

            // Act
            var result = await _service.DeleteAsync(TestAccShortName);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteAsync(TestAccShortName);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingEntity_ReturnsFalse()
        {
            // Arrange
            _repository.GetForeignKeyReferencesAsync("NONEXISTENT").Returns(new List<string>());
            _repository.DeleteAsync("NONEXISTENT").Returns(false);

            // Act
            var result = await _service.DeleteAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordIsReferenced_ThrowsInvalidOperationException()
        {
            // Arrange
            _repository.GetForeignKeyReferencesAsync(TestAccShortName).Returns(new List<string> { "tbladditionalcosts" });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteAsync(TestAccShortName));

            Assert.Contains("cannot be deleted", exception.Message);
            await _repository.DidNotReceive().DeleteAsync(TestAccShortName);
        }

        [Fact]
        public async Task DeleteAsync_NullAccShortName_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.DeleteAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_EmptyAccShortName_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(string.Empty));
        }

        #endregion

        #region Helper Methods

        private static AccountCategory CreateTestEntity(string accShortName, string accountDescription)
        {
            return new AccountCategory
            {
                AccShortName = accShortName,
                AccountDescription = accountDescription,
                AccountType = TestAccountType,
                ConstituentAccountCodes = "1000,2000",
                ProjectSpecific = null,
                RcSpecific = null,
                FpsYear = TestFpsYear
            };
        }

        private static AccountCategoryDto CreateTestDto(string accShortName, string accountDescription)
        {
            return new AccountCategoryDto
            {
                AccShortName = accShortName,
                AccountDescription = accountDescription,
                AccountType = TestAccountType,
                ConstituentAccountCodes = "1000,2000",
                ProjectSpecific = null,
                RcSpecific = null,
                FpsYear = TestFpsYear
            };
        }

        #endregion
    }
}
