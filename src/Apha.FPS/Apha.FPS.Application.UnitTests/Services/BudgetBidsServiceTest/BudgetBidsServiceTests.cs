using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.BudgetBidsServiceTest
{
    public class BudgetBidsServiceTests
    {
        private readonly IBudgetBidsRepository _repository;
        private readonly IMapper _mapper;
        private readonly IFpsRequestContext _requestContext;
        private readonly BudgetBidsService _sut;

        public BudgetBidsServiceTests()
        {
            _repository     = Substitute.For<IBudgetBidsRepository>();
            _mapper         = Substitute.For<IMapper>();
            _requestContext = Substitute.For<IFpsRequestContext>();
            _requestContext.FpsYear.Returns(2024);
            _requestContext.UserEmailId.Returns("test@example.com");
            _sut = new BudgetBidsService(_repository, _mapper, _requestContext);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRequestContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BudgetBidsService(_repository, _mapper, null!));
        }

        #endregion

        #region GetBidViewAsync Tests

        [Fact]
        public async Task GetBidViewAsync_WithData_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<BidView>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 }
            };
            var dtos = new List<BidViewDto>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 }
            };
            _repository.GetBidViewAsync("WG01").Returns(entities);
            _mapper.Map<List<BidViewDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            await _repository.Received(1).GetBidViewAsync("WG01");
        }

        [Fact]
        public async Task GetBidViewAsync_WithNull_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetBidViewAsync(null!));
        }

        [Fact]
        public async Task GetBidViewAsync_WithEmptyWorkgroup_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetBidViewAsync(""));
        }

        [Fact]
        public async Task GetBidViewAsync_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetBidViewAsync("WG01").Returns(new List<BidView>());
            _mapper.Map<List<BidViewDto>>(Arg.Any<List<BidView>>()).Returns(new List<BidViewDto>());

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBidByIdAsync Tests

        [Fact]
        public async Task GetBidByIdAsync_WithExistingBid_ReturnsMappedDto()
        {
            // Arrange
            var entity = new Bid { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 };
            var dto    = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 };
            _repository.GetBidByIdAsync("WG01", "ACC1").Returns(entity);
            _mapper.Map<BidDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetBidByIdAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ACC1", result.Account);
        }

        [Fact]
        public async Task GetBidByIdAsync_WithNonExistingBid_ReturnsNull()
        {
            // Arrange
            _repository.GetBidByIdAsync("WG01", "NOTEXIST").Returns((Bid?)null);
            _mapper.Map<BidDto>(null).Returns((BidDto?)null);

            // Act
            var result = await _sut.GetBidByIdAsync("WG01", "NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddBidAsync Tests

        [Fact]
        public async Task AddBidAsync_WithAuthorizedUserAndNewAccount_ReturnsMappedDto()
        {
            // Arrange
            var bidDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var entity = new Bid   { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var added  = new Bid   { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 };
            var resultDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = 2024 };

            _repository.IsAuthorizedAsync("WG01").Returns(true);
            _repository.GetBidByIdAsync("WG01", "ACC1").Returns((Bid?)null);
            _mapper.Map<Bid>(bidDto).Returns(entity);
            _repository.AddBidAsync(entity).Returns(added);
            _mapper.Map<BidDto>(added).Returns(resultDto);

            // Act
            var result = await _sut.AddBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ACC1", result.Account);
        }

        [Fact]
        public async Task AddBidAsync_WithNullBid_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddBidAsync(null!));
        }

        [Fact]
        public async Task AddBidAsync_WithNegativeGenBid_ThrowsArgumentOutOfRangeException()
        {
            var bidDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = -1m };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.AddBidAsync(bidDto));
        }

        [Fact]
        public async Task AddBidAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var bidDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _repository.IsAuthorizedAsync("WG01").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.AddBidAsync(bidDto));
        }

        [Fact]
        public async Task AddBidAsync_WhenAccountAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var bidDto  = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var existing = new Bid { WorkgroupName = "WG01", Account = "ACC1" };
            _repository.IsAuthorizedAsync("WG01").Returns(true);
            _repository.GetBidByIdAsync("WG01", "ACC1").Returns(existing);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddBidAsync(bidDto));
        }

        #endregion

        #region UpdateBidAsync Tests

        [Fact]
        public async Task UpdateBidAsync_WithAuthorizedUserAndExistingBid_ReturnsMappedDto()
        {
            // Arrange
            var bidDto   = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var entity   = new Bid   { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var existing = new Bid   { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var updated  = new Bid   { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m, FpsYear = 2024 };
            var resultDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 200m };

            _repository.IsAuthorizedAsync("WG01").Returns(true);
            _repository.GetBidByIdAsync("WG01", "ACC1").Returns(existing);
            _mapper.Map<Bid>(bidDto).Returns(entity);
            _repository.UpdateBidAsync(entity).Returns(updated);
            _mapper.Map<BidDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200m, result.GenBid);
        }

        [Fact]
        public async Task UpdateBidAsync_WithNullBid_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateBidAsync(null!));
        }

        [Fact]
        public async Task UpdateBidAsync_WithNegativeGenBid_ThrowsArgumentOutOfRangeException()
        {
            var bidDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = -5m };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.UpdateBidAsync(bidDto));
        }

        [Fact]
        public async Task UpdateBidAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var bidDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _repository.IsAuthorizedAsync("WG01").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateBidAsync(bidDto));
        }

        [Fact]
        public async Task UpdateBidAsync_WhenBidNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var bidDto = new BidDto { WorkgroupName = "WG01", Account = "ACC1", GenBid = 100m };
            _repository.IsAuthorizedAsync("WG01").Returns(true);
            _repository.GetBidByIdAsync("WG01", "ACC1").Returns((Bid?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateBidAsync(bidDto));
        }

        #endregion

        #region DeleteBidAsync Tests

        [Fact]
        public async Task DeleteBidAsync_WithAuthorizedUserAndExistingBid_ReturnsTrue()
        {
            // Arrange
            _repository.IsAuthorizedAsync("WG01").Returns(true);
            _repository.DeleteBidAsync("WG01", "ACC1").Returns(true);

            // Act
            var result = await _sut.DeleteBidAsync("WG01", "ACC1");

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteBidAsync("WG01", "ACC1");
        }

        [Fact]
        public async Task DeleteBidAsync_WithNullWorkgroup_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteBidAsync(null!, "ACC1"));
        }

        [Fact]
        public async Task DeleteBidAsync_WithEmptyWorkgroup_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteBidAsync("", "ACC1"));
        }

        [Fact]
        public async Task DeleteBidAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _repository.IsAuthorizedAsync("WG01").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteBidAsync("WG01", "ACC1"));
        }

        [Fact]
        public async Task DeleteBidAsync_WhenBidNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.IsAuthorizedAsync("WG01").Returns(true);
            _repository.DeleteBidAsync("WG01", "ACC1").Returns(false);

            // Act
            var result = await _sut.DeleteBidAsync("WG01", "ACC1");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithData_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<AccountCategory>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Description 1", RcSpecific = -1 }
            };
            var dtos = new List<AccountCategoryDto>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Description 1" }
            };
            _repository.GetAccountCategoriesAsync().Returns(entities);
            _mapper.Map<List<AccountCategoryDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            await _repository.Received(1).GetAccountCategoriesAsync();
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetAccountCategoriesAsync().Returns(new List<AccountCategory>());
            _mapper.Map<List<AccountCategoryDto>>(Arg.Any<List<AccountCategory>>()).Returns(new List<AccountCategoryDto>());

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
