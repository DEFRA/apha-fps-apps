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
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.DivisionControllerTest
{
    public class DivisionControllerTests
    {
        private readonly IDivisionService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly DivisionController _controller;

        public DivisionControllerTests()
        {
            _serviceMock = Substitute.For<IDivisionService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new DivisionController(_serviceMock, _mapperMock);
        }

        #region GetAllDivisionsAsync

        [Fact]
        public async Task GetAllDivisionsAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<DivisionDto>
            {
                new DivisionDto { DivName = "Division 1", AgencyId = 1 },
                new DivisionDto { DivName = "Division 2", AgencyId = 2 }
            };
            var mappedResult = new List<DivisionRes>
            {
                new DivisionRes { DivName = "Division 1", AgencyId = 1 },
                new DivisionRes { DivName = "Division 2", AgencyId = 2 }
            };

            _serviceMock.GetAllDivisionsAsync().Returns(serviceResult);
            _mapperMock.Map<List<DivisionRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllDivisionsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.GetAllDivisionsAsync().Returns((List<DivisionDto>)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllDivisionsAsync());
        }

        #endregion

        #region GetAllDivisionsPagedAsync

        [Fact]
        public async Task GetAllDivisionsPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var divisionDtos = new List<DivisionDto>
            {
                new DivisionDto { DivName = "Division 1", AgencyId = 1, CentOverhead = 1000.50m }
            };
            var paginationData = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 1,
                TotalPages = 1
            };
            var serviceResult = new PaginatedResult<DivisionDto>(divisionDtos, paginationData);

            var expectedApiResponse = new PaginationRes<DivisionRes>
            {
                Data = new List<DivisionRes>
                {
                    new DivisionRes { DivName = "Division 1", AgencyId = 1, CentOverhead = 1000.50m }
                },
                PaginationData = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1,
                    TotalPages = 1
                }
            };

            _serviceMock.GetAllDivisionsPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionRes>>(serviceResult).Returns(expectedApiResponse);

            // Act
            var result = await _controller.GetAllDivisionsPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedApiResponse, okResult.Value);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _serviceMock.GetAllDivisionsPagedAsync(query).Returns((PaginatedResult<DivisionDto>)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllDivisionsPagedAsync(query));
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_WithFilterAndSorting_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "DivName",
                Descending = true
            };

            var divisionDtos = new List<DivisionDto>
            {
                new DivisionDto { DivName = "Division Z", AgencyId = 1 },
                new DivisionDto { DivName = "Division Y", AgencyId = 2 }
            };
            var paginationData = new PaginationDto
            {
                PageNumber = 2,
                PageSize = 5,
                TotalRecords = 10,
                TotalPages = 2
            };
            var serviceResult = new PaginatedResult<DivisionDto>(divisionDtos, paginationData);

            var expectedApiResponse = new PaginationRes<DivisionRes>
            {
                Data = new List<DivisionRes>
                {
                    new DivisionRes { DivName = "Division Z", AgencyId = 1 },
                    new DivisionRes { DivName = "Division Y", AgencyId = 2 }
                },
                PaginationData = new Pagination
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalRecords = 10,
                    TotalPages = 2
                }
            };

            _serviceMock.GetAllDivisionsPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionRes>>(serviceResult).Returns(expectedApiResponse);

            // Act
            var result = await _controller.GetAllDivisionsPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<DivisionRes>>(okResult.Value);
            Assert.Equal(2, response.PaginationData.PageNumber);
            Assert.Equal(5, response.PaginationData.PageSize);
        }

        #endregion

        #region GetDivisionByNameAsync

        [Fact]
        public async Task GetDivisionByNameAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var divName = "TestDivision";
            var dto = new DivisionDto
            {
                DivName = divName,
                AgencyId = 1,
                CentOverhead = 5000.75m
            };
            var mapped = new DivisionRes
            {
                DivName = divName,
                AgencyId = 1,
                CentOverhead = 5000.75m,
                AgencyName = "Test Agency"
            };

            _serviceMock.GetDivisionByNameAsync(divName).Returns(dto);
            _mapperMock.Map<DivisionRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetDivisionByNameAsync(divName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            var divName = "NonExistentDivision";
            _serviceMock.GetDivisionByNameAsync(divName).Returns((DivisionDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetDivisionByNameAsync(divName));
            Assert.Contains(divName, exception.Message);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_CaseInsensitive_ReturnsOk()
        {
            // Arrange
            var divName = "testdivision";
            var dto = new DivisionDto { DivName = "TestDivision", AgencyId = 1 };
            var mapped = new DivisionRes { DivName = "TestDivision", AgencyId = 1 };

            _serviceMock.GetDivisionByNameAsync(divName).Returns(dto);
            _mapperMock.Map<DivisionRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetDivisionByNameAsync(divName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region CreateDivisionAsync

        [Fact]
        public async Task CreateDivisionAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = new DivisionReq
            {
                DivName = "New Division",
                AgencyId = 1,
                CentOverhead = 2500.00m
            };
            var dto = new DivisionDto
            {
                DivName = "New Division",
                AgencyId = 1,
                CentOverhead = 2500.00m
            };
            var resultDto = new DivisionDto
            {
                DivisionId = 100,
                DivName = "New Division",
                AgencyId = 1,
                CentOverhead = 2500.00m
            };
            var mapped = new DivisionRes
            {
                DivisionId = 100,
                DivName = "New Division",
                AgencyId = 1,
                CentOverhead = 2500.00m
            };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.CreateDivisionAsync(dto).Returns(resultDto);
            _mapperMock.Map<DivisionRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.CreateDivisionAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).CreateDivisionAsync(dto);
        }

        [Fact]
        public async Task CreateDivisionAsync_Error_ServiceThrows()
        {
            // Arrange
            var req = new DivisionReq
            {
                DivName = "Invalid Division",
                AgencyId = 999
            };
            var dto = new DivisionDto
            {
                DivName = "Invalid Division",
                AgencyId = 999
            };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.CreateDivisionAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateDivisionAsync(req));
        }

        [Fact]
        public async Task CreateDivisionAsync_WithNullCentOverhead_ReturnsOk()
        {
            // Arrange
            var req = new DivisionReq
            {
                DivName = "Division Without Overhead",
                AgencyId = 1,
                CentOverhead = null
            };
            var dto = new DivisionDto
            {
                DivName = "Division Without Overhead",
                AgencyId = 1,
                CentOverhead = null
            };
            var resultDto = new DivisionDto
            {
                DivisionId = 101,
                DivName = "Division Without Overhead",
                AgencyId = 1,
                CentOverhead = null
            };
            var mapped = new DivisionRes
            {
                DivisionId = 101,
                DivName = "Division Without Overhead",
                AgencyId = 1,
                CentOverhead = null
            };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.CreateDivisionAsync(dto).Returns(resultDto);
            _mapperMock.Map<DivisionRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.CreateDivisionAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<DivisionRes>(okResult.Value);
            Assert.Null(response.CentOverhead);
        }

        #endregion

        #region UpdateDivisionAsync

        [Fact]
        public async Task UpdateDivisionAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var divName = "ExistingDivision";
            var req = new DivisionReq
            {
                DivName = divName,
                AgencyId = 1,
                CentOverhead = 3000.00m
            };
            var dto = new DivisionDto
            {
                DivName = divName,
                AgencyId = 1,
                CentOverhead = 3000.00m
            };
            var resultDto = new DivisionDto
            {
                DivisionId = 50,
                DivName = divName,
                AgencyId = 1,
                CentOverhead = 3000.00m
            };
            var mapped = new DivisionRes
            {
                DivisionId = 50,
                DivName = divName,
                AgencyId = 1,
                CentOverhead = 3000.00m
            };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.UpdateDivisionAsync(divName, dto).Returns(resultDto);
            _mapperMock.Map<DivisionRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.UpdateDivisionAsync(divName, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).UpdateDivisionAsync(divName, dto);
        }

        [Fact]
        public async Task UpdateDivisionAsync_Error_ServiceThrows()
        {
            // Arrange
            var divName = "TestDivision";
            var req = new DivisionReq
            {
                DivName = divName,
                AgencyId = 1
            };
            var dto = new DivisionDto
            {
                DivName = divName,
                AgencyId = 1
            };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.UpdateDivisionAsync(divName, dto).Throws(new Exception("Update failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateDivisionAsync(divName, req));
        }

        [Fact]
        public async Task UpdateDivisionAsync_UpdatingCentOverhead_ReturnsOk()
        {
            // Arrange
            var divName = "DivisionToUpdate";
            var req = new DivisionReq
            {
                DivName = divName,
                AgencyId = 2,
                CentOverhead = 7500.25m
            };
            var dto = new DivisionDto
            {
                DivName = divName,
                AgencyId = 2,
                CentOverhead = 7500.25m
            };
            var resultDto = new DivisionDto
            {
                DivisionId = 75,
                DivName = divName,
                AgencyId = 2,
                CentOverhead = 7500.25m
            };
            var mapped = new DivisionRes
            {
                DivisionId = 75,
                DivName = divName,
                AgencyId = 2,
                CentOverhead = 7500.25m
            };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.UpdateDivisionAsync(divName, dto).Returns(resultDto);
            _mapperMock.Map<DivisionRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.UpdateDivisionAsync(divName, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<DivisionRes>(okResult.Value);
            Assert.Equal(7500.25m, response.CentOverhead);
        }

        #endregion

        #region DeleteDivisionAsync

        [Fact]
        public async Task DeleteDivisionAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var divName = "DivisionToDelete";
            _serviceMock.DeleteDivisionAsync(divName).Returns(true);

            // Act
            var result = await _controller.DeleteDivisionAsync(divName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteDivisionAsync(divName);
        }

        [Fact]
        public async Task DeleteDivisionAsync_NullOrEmpty_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteDivisionAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteDivisionAsync("   "));
        }

        [Fact]
        public async Task DeleteDivisionAsync_NullDivName_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteDivisionAsync(null!));
        }

        [Fact]
        public async Task DeleteDivisionAsync_NotFound_ThrowsArgumentException()
        {
            // Arrange
            var divName = "NonExistentDivision";
            _serviceMock.DeleteDivisionAsync(divName).Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.DeleteDivisionAsync(divName));
            Assert.Contains(divName, exception.Message);
            Assert.Contains("not found for deletion", exception.Message);
        }

        [Fact]
        public async Task DeleteDivisionAsync_ServiceReturnsFalse_ThrowsArgumentException()
        {
            // Arrange
            var divName = "TestDivision";
            _serviceMock.DeleteDivisionAsync(divName).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteDivisionAsync(divName));
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_NullDivisionService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new DivisionController(_serviceMock, null!));
        }

        #endregion

        #region Integration and Edge Case Tests

        [Theory]
        [InlineData("Division A")]
        [InlineData("DIVISION B")]
        [InlineData("division c")]
        public async Task GetDivisionByNameAsync_VariousDivisionNames_CallsService(string divName)
        {
            // Arrange
            var dto = new DivisionDto { DivName = divName, AgencyId = 1 };
            var mapped = new DivisionRes { DivName = divName, AgencyId = 1 };

            _serviceMock.GetDivisionByNameAsync(divName).Returns(dto);
            _mapperMock.Map<DivisionRes>(dto).Returns(mapped);

            // Act
            await _controller.GetDivisionByNameAsync(divName);

            // Assert
            await _serviceMock.Received(1).GetDivisionByNameAsync(divName);
        }

        [Theory]
        [InlineData("Division1")]
        [InlineData("Division2")]
        [InlineData("Division3")]
        public async Task DeleteDivisionAsync_VariousDivisionNames_CallsService(string divName)
        {
            // Arrange
            _serviceMock.DeleteDivisionAsync(divName).Returns(true);

            // Act
            await _controller.DeleteDivisionAsync(divName);

            // Assert
            await _serviceMock.Received(1).DeleteDivisionAsync(divName);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_EmptyList_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<DivisionDto>();
            var mappedResult = new List<DivisionRes>();

            _serviceMock.GetAllDivisionsAsync().Returns(serviceResult);
            _mapperMock.Map<List<DivisionRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllDivisionsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var divisions = Assert.IsType<List<DivisionRes>>(okResult.Value);
            Assert.Empty(divisions);
        }

        [Fact]
        public async Task CreateDivisionAsync_CallsMapperAndService_InCorrectOrder()
        {
            // Arrange
            var req = new DivisionReq { DivName = "Test", AgencyId = 1 };
            var dto = new DivisionDto { DivName = "Test", AgencyId = 1 };
            var resultDto = new DivisionDto { DivName = "Test", AgencyId = 1 };
            var mapped = new DivisionRes { DivName = "Test", AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.CreateDivisionAsync(dto).Returns(resultDto);
            _mapperMock.Map<DivisionRes>(resultDto).Returns(mapped);

            // Act
            await _controller.CreateDivisionAsync(req);

            // Assert
            _mapperMock.Received(1).Map<DivisionDto>(req);
            await _serviceMock.Received(1).CreateDivisionAsync(dto);
            _mapperMock.Received(1).Map<DivisionRes>(resultDto);
        }

        [Fact]
        public async Task UpdateDivisionAsync_CallsMapperAndService_InCorrectOrder()
        {
            // Arrange
            var divName = "TestDivision";
            var req = new DivisionReq { DivName = divName, AgencyId = 1 };
            var dto = new DivisionDto { DivName = divName, AgencyId = 1 };
            var resultDto = new DivisionDto { DivName = divName, AgencyId = 1 };
            var mapped = new DivisionRes { DivName = divName, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(req).Returns(dto);
            _serviceMock.UpdateDivisionAsync(divName, dto).Returns(resultDto);
            _mapperMock.Map<DivisionRes>(resultDto).Returns(mapped);

            // Act
            await _controller.UpdateDivisionAsync(divName, req);

            // Assert
            _mapperMock.Received(1).Map<DivisionDto>(req);
            await _serviceMock.Received(1).UpdateDivisionAsync(divName, dto);
            _mapperMock.Received(1).Map<DivisionRes>(resultDto);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var divisionDtos = new List<DivisionDto>();
            var paginationData = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 0,
                TotalPages = 0
            };
            var serviceResult = new PaginatedResult<DivisionDto>(divisionDtos, paginationData);

            var expectedApiResponse = new PaginationRes<DivisionRes>
            {
                Data = new List<DivisionRes>(),
                PaginationData = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0,
                    TotalPages = 0
                }
            };

            _serviceMock.GetAllDivisionsPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionRes>>(serviceResult).Returns(expectedApiResponse);

            // Act
            var result = await _controller.GetAllDivisionsPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<DivisionRes>>(okResult.Value);
            Assert.Empty(response.Data);
            Assert.Equal(0, response.PaginationData.TotalRecords);
        }

        #endregion
    }
}
