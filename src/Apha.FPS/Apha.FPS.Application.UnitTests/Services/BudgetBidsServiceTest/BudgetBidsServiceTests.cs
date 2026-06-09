using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using Moq;

namespace Apha.FPS.Application.UnitTests.Services.BudgetBidsServiceTest
{
    public class BudgetBidsServiceTests
    {
        private const string DefaultWorkGroup = "WG01";
        private const string DefaultAccount   = "ACC1";

        private readonly Mock<IBudgetBidsRepository> _repositoryMock;
        private readonly Mock<IMapper>               _mapperMock;
        private readonly Mock<IFpsRequestContext>    _requestContextMock;
        private readonly BudgetBidsService           _sut;

        public BudgetBidsServiceTests()
        {
            _repositoryMock     = new Mock<IBudgetBidsRepository>();
            _mapperMock         = new Mock<IMapper>();
            _requestContextMock = new Mock<IFpsRequestContext>();
            _sut = new BudgetBidsService(
                _repositoryMock.Object,
                _mapperMock.Object,
                _requestContextMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsService(null!, _mapperMock.Object, _requestContextMock.Object));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsService(_repositoryMock.Object, null!, _requestContextMock.Object));
        }

        [Fact]
        public void Constructor_WithNullRequestContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsService(_repositoryMock.Object, _mapperMock.Object, null!));
        }

        #endregion

        #region DeleteBidAsync — related purchases validation

        [Fact]
        public async Task DeleteBidAsync_WhenRelatedPurchasesExist_ThrowsInvalidOperationException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.IsAuthorizedAsync(DefaultWorkGroup))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            Assert.Equal(
                "This record cannot be deleted as it has a related entry in the Purchase table.",
                ex.Message);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenNoRelatedPurchases_CallsRepositoryDelete()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.IsAuthorizedAsync(DefaultWorkGroup))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(r => r.DeleteBidAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(r => r.DeleteBidAsync(DefaultWorkGroup, DefaultAccount), Times.Once);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenRelatedPurchasesExist_DoesNotCallRepositoryDelete()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.IsAuthorizedAsync(DefaultWorkGroup))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            _repositoryMock.Verify(r => r.DeleteBidAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.IsAuthorizedAsync(DefaultWorkGroup))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            _repositoryMock.Verify(r => r.HasRelatedPurchasesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.DeleteBidAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBidAsync_WithNullOrWhiteSpaceWorkGroupName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteBidAsync("", DefaultAccount));
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteBidAsync("   ", DefaultAccount));
        }

        [Fact]
        public async Task DeleteBidAsync_WhenRelatedPurchasesExist_HasRelatedPurchasesCalledOnce()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.IsAuthorizedAsync(DefaultWorkGroup))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            _repositoryMock.Verify(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount), Times.Once);
        }

        #endregion
    }
}
