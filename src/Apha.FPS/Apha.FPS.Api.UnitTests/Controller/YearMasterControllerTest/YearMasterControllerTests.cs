using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.YearMasterControllerTest
{
    public class YearMasterControllerTests
    {
        private readonly IYearMasterService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly YearMasterController _controller;

        public YearMasterControllerTests()
        {
            _serviceMock = Substitute.For<IYearMasterService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new YearMasterController(_serviceMock, _mapperMock);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new YearMasterController(null!, _mapperMock)
            );

            Assert.Equal("yearMasterService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new YearMasterController(_serviceMock, null!)
            );

            Assert.Equal("mapper", exception.ParamName);
        }

        #endregion

        #region GetAllYearMastersAsync (Non-Paginated)

        [Fact]
        public async Task GetAllYearMastersAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, YearStatus = "Open" },
                new YearMasterDto { FpsYear = 2025, YearStatus = "Planned" }
            };
            var mappedResult = new List<YearMasterRes>();

            _serviceMock.GetAllYearMastersAsync().Returns(serviceResult);
            _mapperMock.Map<List<YearMasterRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllYearMastersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            await _serviceMock.Received(1).GetAllYearMastersAsync();
            _mapperMock.Received(1).Map<List<YearMasterRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_EdgeCase_EmptyList_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<YearMasterDto>();
            var mappedResult = new List<YearMasterRes>();

            _serviceMock.GetAllYearMastersAsync().Returns(serviceResult);
            _mapperMock.Map<List<YearMasterRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllYearMastersAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_EdgeCase_NullResult_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.GetAllYearMastersAsync().Returns((IEnumerable<YearMasterDto>)null!);

            // Act
            var result = await _controller.GetAllYearMastersAsync();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Year Master records not found", notFoundResult.Value);

            await _serviceMock.Received(1).GetAllYearMastersAsync();
            _mapperMock.DidNotReceive().Map<List<YearMasterRes>>(Arg.Any<IEnumerable<YearMasterDto>>());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetAllYearMastersAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllYearMastersAsync());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<YearMasterDto>();
            _serviceMock.GetAllYearMastersAsync().Returns(serviceResult);
            _mapperMock.Map<List<YearMasterRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllYearMastersAsync());
        }

        #endregion

        #region GetAllYearMastersPagedAsync

        [Fact]
        public async Task GetAllYearMastersPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var serviceResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2024, YearStatus = "Open" }
                },
                PaginationData = new PaginationDto
                {
                    TotalRecords = 1,
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10
                }
            };

            var mappedResult = new PaginationRes<YearMasterRes>();

            _serviceMock.GetAllYearMastersAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<YearMasterRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllYearMastersPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            await _serviceMock.Received(1).GetAllYearMastersAsync(query);
            _mapperMock.Received(1).Map<PaginationRes<YearMasterRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_EdgeCase_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };
            var mappedResult = new PaginationRes<YearMasterRes>();

            _serviceMock.GetAllYearMastersAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<YearMasterRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllYearMastersPagedAsync(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_EdgeCase_NullResult_ReturnsNotFound()
        {
            // Arrange
            var query = new QueryParameters<int> { Page = 1, PageSize = 10 };
            _serviceMock.GetAllYearMastersAsync(query).Returns((PaginatedResult<YearMasterDto>)null!);

            // Act
            var result = await _controller.GetAllYearMastersPagedAsync(query);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Year Master records not found", notFoundResult.Value);

            await _serviceMock.Received(1).GetAllYearMastersAsync(query);
            _mapperMock.DidNotReceive().Map<PaginationRes<YearMasterRes>>(Arg.Any<PaginatedResult<YearMasterDto>>());
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithFilter_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var serviceResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2024, YearStatus = "Open" }
                }
            };
            var mappedResult = new PaginationRes<YearMasterRes>();

            _serviceMock.GetAllYearMastersAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<YearMasterRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllYearMastersPagedAsync(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithMultiplePages_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 2,
                PageSize = 5
            };

            var serviceResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2019 },
                    new YearMasterDto { FpsYear = 2018 }
                },
                PaginationData = new PaginationDto
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 12
                }
            };
            var mappedResult = new PaginationRes<YearMasterRes>();

            _serviceMock.GetAllYearMastersAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<YearMasterRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllYearMastersPagedAsync(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_Error_ServiceThrows()
        {
            // Arrange
            var query = new QueryParameters<int> { Page = 1, PageSize = 10 };
            _serviceMock.GetAllYearMastersAsync(query).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllYearMastersPagedAsync(query));
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_Error_MapperThrows()
        {
            // Arrange
            var query = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<YearMasterDto>();

            _serviceMock.GetAllYearMastersAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<YearMasterRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllYearMastersPagedAsync(query));
        }

        #endregion

        #region GetYearMasterById

        [Fact]
        public async Task GetYearMasterById_HappyPath_ReturnsOk()
        {
            // Arrange
            var fpsYear = 2024;
            var dto = new YearMasterDto
            {
                FpsYear = 2024,
                FpsYearCode = "2024",
                YearStatus = "Open",
                Active = true
            };
            var mapped = new YearMasterRes();

            _serviceMock.GetYearMasterByIdAsync(fpsYear).Returns(dto);
            _mapperMock.Map<YearMasterRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetYearMasterById(fpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);

            await _serviceMock.Received(1).GetYearMasterByIdAsync(fpsYear);
            _mapperMock.Received(1).Map<YearMasterRes>(dto);
        }

        [Fact]
        public async Task GetYearMasterById_WithClosedYear_ReturnsOk()
        {
            // Arrange
            var fpsYear = 2023;
            var dto = new YearMasterDto
            {
                FpsYear = 2023,
                FpsYearCode = "2023",
                YearStatus = "Closed",
                Active = true
            };
            var mapped = new YearMasterRes();

            _serviceMock.GetYearMasterByIdAsync(fpsYear).Returns(dto);
            _mapperMock.Map<YearMasterRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetYearMasterById(fpsYear);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetYearMasterById_WithPlannedYear_ReturnsOk()
        {
            // Arrange
            var fpsYear = 2025;
            var dto = new YearMasterDto
            {
                FpsYear = 2025,
                FpsYearCode = "2025",
                YearStatus = "Planned",
                Active = true
            };
            var mapped = new YearMasterRes();

            _serviceMock.GetYearMasterByIdAsync(fpsYear).Returns(dto);
            _mapperMock.Map<YearMasterRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetYearMasterById(fpsYear);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetYearMasterById_EdgeCase_NullResult_ReturnsNotFound()
        {
            // Arrange
            var fpsYear = 9999;
            _serviceMock.GetYearMasterByIdAsync(fpsYear).Returns((YearMasterDto?)null);

            // Act
            var result = await _controller.GetYearMasterById(fpsYear);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Year Master record with FPS Year: {fpsYear} not found", notFoundResult.Value);

            await _serviceMock.Received(1).GetYearMasterByIdAsync(fpsYear);
            _mapperMock.DidNotReceive().Map<YearMasterRes>(Arg.Any<YearMasterDto>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2024)]
        public async Task GetYearMasterById_EdgeCase_InvalidYear_ReturnsNotFound(int invalidYear)
        {
            // Arrange
            _serviceMock.GetYearMasterByIdAsync(invalidYear).Returns((YearMasterDto?)null);

            // Act
            var result = await _controller.GetYearMasterById(invalidYear);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);

            await _serviceMock.Received(1).GetYearMasterByIdAsync(invalidYear);
        }

        [Fact]
        public async Task GetYearMasterById_Error_ServiceThrows()
        {
            // Arrange
            var fpsYear = 2024;
            _serviceMock.GetYearMasterByIdAsync(fpsYear).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetYearMasterById(fpsYear));

            await _serviceMock.Received(1).GetYearMasterByIdAsync(fpsYear);
        }

        [Fact]
        public async Task GetYearMasterById_Error_MapperThrows()
        {
            // Arrange
            var fpsYear = 2024;
            var dto = new YearMasterDto { FpsYear = 2024 };

            _serviceMock.GetYearMasterByIdAsync(fpsYear).Returns(dto);
            _mapperMock.Map<YearMasterRes>(dto).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetYearMasterById(fpsYear));

            await _serviceMock.Received(1).GetYearMasterByIdAsync(fpsYear);
        }

        #endregion
    }
}
