using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.PurchasesServiceTest
{
    public class PurchasesServiceTests
    {
        private readonly IPurchasesRepository _repository;
        private readonly IMapper _mapper;
        private readonly IFpsRequestContext _requestContext;
        private readonly PurchasesService _sut;

        public PurchasesServiceTests()
        {
            _repository     = Substitute.For<IPurchasesRepository>();
            _mapper         = Substitute.For<IMapper>();
            _requestContext = Substitute.For<IFpsRequestContext>();
            _requestContext.FpsYear.Returns(2024);
            _requestContext.UserEmailId.Returns("test@example.com");
            _sut = new PurchasesService(_repository, _mapper, _requestContext);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRequestContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PurchasesService(_repository, _mapper, null!));
        }

        #endregion

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithData_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = 2024 }
            };
            var dtos = new List<PurchaseDto>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m }
            };
            _repository.GetPurchasesAsync("WG01", "ACC1").Returns(entities);
            _mapper.Map<List<PurchaseDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            await _repository.Received(1).GetPurchasesAsync("WG01", "ACC1");
        }

        [Fact]
        public async Task GetPurchasesAsync_WithNull_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetPurchasesAsync(null!, "ACC1"));
        }

        [Fact]
        public async Task GetPurchasesAsync_WithEmptyWorkgroup_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetPurchasesAsync("", "ACC1"));
        }

        [Fact]
        public async Task GetPurchasesAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetPurchasesAsync("WG01", "ACC1").Returns(new List<Purchase>());
            _mapper.Map<List<PurchaseDto>>(Arg.Any<List<Purchase>>()).Returns(new List<PurchaseDto>());

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetPurchaseByIdAsync Tests

        [Fact]
        public async Task GetPurchaseByIdAsync_WithExistingPurchase_ReturnsMappedDto()
        {
            // Arrange
            var entity = new Purchase { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = 2024 };
            var dto    = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(entity);
            _mapper.Map<PurchaseDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Item A", result.ItemDescription);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_WithNonExistingPurchase_ReturnsNull()
        {
            // Arrange
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST").Returns((Purchase?)null);
            _mapper.Map<PurchaseDto>(null).Returns((PurchaseDto?)null);

            // Act
            var result = await _sut.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddPurchaseAsync Tests

        [Fact]
        public async Task AddPurchaseAsync_WithAuthorizedUserAndNewItem_ReturnsMappedDto()
        {
            // Arrange
            var dto     = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var entity  = new Purchase   { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var added   = new Purchase   { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = 2024 };
            var resultDto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };

            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns((Purchase?)null);
            _mapper.Map<Purchase>(dto).Returns(entity);
            _repository.AddPurchaseAsync(entity).Returns(added);
            _mapper.Map<PurchaseDto>(added).Returns(resultDto);

            // Act
            var result = await _sut.AddPurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Item A", result.ItemDescription);
        }

        [Fact]
        public async Task AddPurchaseAsync_WithNullPurchase_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddPurchaseAsync(null!));
        }

        [Fact]
        public async Task AddPurchaseAsync_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
        {
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = -1m };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.AddPurchaseAsync(dto));
        }

        [Fact]
        public async Task AddPurchaseAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.AddPurchaseAsync(dto));
        }

        [Fact]
        public async Task AddPurchaseAsync_WhenItemAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto      = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var existing = new Purchase { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(existing);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddPurchaseAsync(dto));
        }

        #endregion

        #region UpdatePurchaseAsync Tests

        [Fact]
        public async Task UpdatePurchaseAsync_WithAuthorizedUserAndExistingItem_ReturnsMappedDto()
        {
            // Arrange
            var dto = new PurchaseDto
            {
                WorkgroupName      = "WG01",
                Account            = "ACC1",
                ItemDescription    = "Item B",
                Amount             = 200m,
                OldItemDescription = "Item A"
            };
            var existing  = new Purchase { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var updated   = new Purchase { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, FpsYear = 2024 };
            var resultDto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };

            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(existing);
            _repository.UpdatePurchaseAsync("WG01", "ACC1", "Item A", "Item B", 200m).Returns(updated);
            _mapper.Map<PurchaseDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Item B", result.ItemDescription);
        }

        [Fact]
        public async Task UpdatePurchaseAsync_UsesItemDescriptionWhenOldDescriptionIsNull()
        {
            // Arrange
            var dto = new PurchaseDto
            {
                WorkgroupName      = "WG01",
                Account            = "ACC1",
                ItemDescription    = "Item A",
                Amount             = 200m,
                OldItemDescription = null
            };
            var existing  = new Purchase { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var updated   = new Purchase { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 200m, FpsYear = 2024 };
            var resultDto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 200m };

            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(existing);
            _repository.UpdatePurchaseAsync("WG01", "ACC1", "Item A", "Item A", 200m).Returns(updated);
            _mapper.Map<PurchaseDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WithNullPurchase_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdatePurchaseAsync(null!));
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
        {
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = -1m };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.UpdatePurchaseAsync(dto));
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdatePurchaseAsync(dto));
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WhenItemNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new PurchaseDto { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns((Purchase?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdatePurchaseAsync(dto));
        }

        #endregion

        #region DeletePurchaseAsync Tests

        [Fact]
        public async Task DeletePurchaseAsync_WithAuthorizedUserAndExistingItem_ReturnsTrue()
        {
            // Arrange
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.DeletePurchaseAsync("WG01", "ACC1", "Item A").Returns(true);

            // Act
            var result = await _sut.DeletePurchaseAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeletePurchaseAsync("WG01", "ACC1", "Item A");
        }

        [Fact]
        public async Task DeletePurchaseAsync_WithNullWorkgroup_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeletePurchaseAsync(null!, "ACC1", "Item A"));
        }

        [Fact]
        public async Task DeletePurchaseAsync_WithEmptyWorkgroup_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeletePurchaseAsync("", "ACC1", "Item A"));
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeletePurchaseAsync("WG01", "ACC1", "Item A"));
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenItemNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.IsAuthorizedAsync("WG01", "test@example.com").Returns(true);
            _repository.DeletePurchaseAsync("WG01", "ACC1", "Item A").Returns(false);

            // Act
            var result = await _sut.DeletePurchaseAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
