using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.DivisionControllerTest
{
    public class DivisionControllerTests
    {
        private readonly IDivisionService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ILogger<DivisionController> _loggerMock;
        private readonly DivisionController _controller;

        public DivisionControllerTests()
        {
            _serviceMock = Substitute.For<IDivisionService>();
            _mapperMock = Substitute.For<IMapper>();
            _loggerMock = Substitute.For<ILogger<DivisionController>>();
            _controller = new DivisionController(_serviceMock, _mapperMock, _loggerMock);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new DivisionController(null!, _mapperMock, _loggerMock)
            );

            Assert.Equal("divisionService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new DivisionController(_serviceMock, null!, _loggerMock)
            );

            Assert.Equal("mapper", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new DivisionController(_serviceMock, _mapperMock, null!)
            );

            Assert.Equal("logger", exception.ParamName);
        }

        #endregion

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new DivisionDto { DivName = "ACDP", DivisionId = 2, AgencyId = 1 }
            };
            var mappedResult = new List<DivisionRes>
            {
                new DivisionRes { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new DivisionRes { DivName = "ACDP", DivisionId = 2, AgencyId = 1 }
            };

            _serviceMock.GetAllDivisionsAsync().Returns(serviceResult);
            _mapperMock.Map<List<DivisionRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllDivisionsAsync();

            // Assert
            var okResult = Assert.IsType<ActionResult<List<DivisionRes>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.Equal(mappedResult, okObjectResult.Value);

            await _serviceMock.Received(1).GetAllDivisionsAsync();
            _mapperMock.Received(1).Map<List<DivisionRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_EmptyList_ReturnsNotFound()
        {
            // Arrange
            var serviceResult = new List<DivisionDto>();

            _serviceMock.GetAllDivisionsAsync().Returns(serviceResult);

            // Act
            var result = await _controller.GetAllDivisionsAsync();

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<DivisionRes>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("No division records found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_NullResult_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.GetAllDivisionsAsync().Returns((List<DivisionDto>)null!);

            // Act
            var result = await _controller.GetAllDivisionsAsync();

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<DivisionRes>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("No division records found", notFoundResult.Value);
        }

        #endregion

        #region GetAllDivisionsPagedAsync Tests

        [Fact]
        public async Task GetAllDivisionsPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<DivisionDto>
            {
                Data = new List<DivisionDto>
                {
                    new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
                },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var mappedResult = new PaginationRes<DivisionRes>
            {
                Data = new List<DivisionRes>
                {
                    new DivisionRes { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetAllDivisionsPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllDivisionsPagedAsync(query);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PaginationRes<DivisionRes>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(mappedResult, okResult.Value);

            await _serviceMock.Received(1).GetAllDivisionsPagedAsync(query);
            _mapperMock.Received(1).Map<PaginationRes<DivisionRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_EmptyData_ReturnsNotFound()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<DivisionDto>
            {
                Data = new List<DivisionDto>(),
                PaginationData = new PaginationDto()
            };

            _serviceMock.GetAllDivisionsPagedAsync(query).Returns(serviceResult);

            // Act
            var result = await _controller.GetAllDivisionsPagedAsync(query);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PaginationRes<DivisionRes>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("No division records found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_PassesCorrectQueryParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                SortBy = "DivName",
                Descending = true,
                Filter = "{\"DivName\":\"VSD\"}"
            };
            var serviceResult = new PaginatedResult<DivisionDto>
            {
                Data = new List<DivisionDto>
                {
                    new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
                },
                PaginationData = new PaginationDto()
            };
            var mappedResult = new PaginationRes<DivisionRes>();

            _serviceMock.GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<DivisionRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetAllDivisionsPagedAsync(query);

            // Assert
            await _serviceMock.Received(1).GetAllDivisionsPagedAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 20 &&
                    q.SortBy == "DivName" &&
                    q.Descending == true &&
                    q.Filter == "{\"DivName\":\"VSD\"}"
                ));
        }

        #endregion

        #region GetDivisionByNameAsync Tests

        [Fact]
        public async Task GetDivisionByNameAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var divName = "VSD";
            var serviceResult = new DivisionDto { DivName = divName, DivisionId = 1, AgencyId = 1 };
            var mappedResult = new DivisionRes { DivName = divName, DivisionId = 1, AgencyId = 1 };

            _serviceMock.GetDivisionByNameAsync(divName).Returns(serviceResult);
            _mapperMock.Map<DivisionRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetDivisionByNameAsync(divName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(mappedResult, okResult.Value);

            await _serviceMock.Received(1).GetDivisionByNameAsync(divName);
            _mapperMock.Received(1).Map<DivisionRes>(serviceResult);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_NotFound_ReturnsNotFound()
        {
            // Arrange
            var divName = "NONEXISTENT";

            _serviceMock.GetDivisionByNameAsync(divName).Returns((DivisionDto)null!);

            // Act
            var result = await _controller.GetDivisionByNameAsync(divName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal($"Division with name '{divName}' not found", notFoundResult.Value);
        }

        #endregion

        #region CreateDivisionAsync Tests

        [Fact]
        public async Task CreateDivisionAsync_HappyPath_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new DivisionReq { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var divisionDto = new DivisionDto { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var createdDivision = new DivisionDto { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var response = new DivisionRes { DivName = "NEW", DivisionId = 99, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.CreateDivisionAsync(divisionDto).Returns(createdDivision);
            _mapperMock.Map<DivisionRes>(createdDivision).Returns(response);

            // Act
            var result = await _controller.CreateDivisionAsync(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(DivisionController.GetDivisionByNameAsync), createdResult.ActionName);
            Assert.Equal(response, createdResult.Value);
            Assert.NotNull(createdResult.RouteValues);
            Assert.True(createdResult.RouteValues.ContainsKey("divName"));
            Assert.Equal("NEW", createdResult.RouteValues["divName"]);

            await _serviceMock.Received(1).CreateDivisionAsync(divisionDto);
        }

        [Fact]
        public async Task CreateDivisionAsync_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateDivisionAsync(null!);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateDivisionAsync_DuplicateDivision_ReturnsBadRequest()
        {
            // Arrange
            var request = new DivisionReq { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var divisionDto = new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.CreateDivisionAsync(divisionDto)
                .Throws(new InvalidOperationException("Unable to add the division name as it is already in use."));

            // Act
            var result = await _controller.CreateDivisionAsync(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateDivisionAsync_InvalidArgument_ReturnsBadRequest()
        {
            // Arrange
            var request = new DivisionReq { DivName = "", DivisionId = 1, AgencyId = 1 };
            var divisionDto = new DivisionDto { DivName = "", DivisionId = 1, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.CreateDivisionAsync(divisionDto)
                .Throws(new ArgumentException("Division name cannot be empty"));

            // Act
            var result = await _controller.CreateDivisionAsync(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateDivisionAsync_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            var request = new DivisionReq { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var divisionDto = new DivisionDto { DivName = "NEW", DivisionId = 99, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.CreateDivisionAsync(divisionDto)
                .Throws(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateDivisionAsync(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region UpdateDivisionAsync Tests

        [Fact]
        public async Task UpdateDivisionAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var divName = "VSD";
            var request = new DivisionReq { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var divisionDto = new DivisionDto { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var updatedDivision = new DivisionDto { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var response = new DivisionRes { DivName = "VSD", DivisionId = 2, AgencyId = 2 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.UpdateDivisionAsync(divName, divisionDto).Returns(updatedDivision);
            _mapperMock.Map<DivisionRes>(updatedDivision).Returns(response);

            // Act
            var result = await _controller.UpdateDivisionAsync(divName, request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(response, okResult.Value);

            await _serviceMock.Received(1).UpdateDivisionAsync(divName, divisionDto);
        }

        [Fact]
        public async Task UpdateDivisionAsync_DivisionNameChange_WithFKReferences_ReturnsBadRequest()
        {
            // Arrange
            var divName = "VSD";
            var request = new DivisionReq { DivName = "NEWNAME", DivisionId = 1, AgencyId = 1 };
            var divisionDto = new DivisionDto { DivName = "NEWNAME", DivisionId = 1, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.UpdateDivisionAsync(divName, divisionDto)
                .Throws(new InvalidOperationException("Unable to edit the division name as it is already in use."));

            // Act
            var result = await _controller.UpdateDivisionAsync(divName, request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateDivisionAsync_DivisionNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var divName = "NONEXISTENT";
            var request = new DivisionReq { DivName = "VSD", DivisionId = 1, AgencyId = 1 };
            var divisionDto = new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.UpdateDivisionAsync(divName, divisionDto)
                .Throws(new InvalidOperationException("Division not found"));

            // Act
            var result = await _controller.UpdateDivisionAsync(divName, request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateDivisionAsync_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            var divName = "VSD";
            var request = new DivisionReq { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var divisionDto = new DivisionDto { DivName = "VSD", DivisionId = 2, AgencyId = 2 };

            _mapperMock.Map<DivisionDto>(request).Returns(divisionDto);
            _serviceMock.UpdateDivisionAsync(divName, divisionDto)
                .Throws(new Exception("Database connection failed"));

            // Act
            var result = await _controller.UpdateDivisionAsync(divName, request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DivisionRes>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region DeleteDivisionAsync Tests

        [Fact]
        public async Task DeleteDivisionAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var divName = "VSD";

            _serviceMock.DeleteDivisionAsync(divName).Returns(true);

            // Act
            var result = await _controller.DeleteDivisionAsync(divName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(true, okResult.Value);

            await _serviceMock.Received(1).DeleteDivisionAsync(divName);
        }

        [Fact]
        public async Task DeleteDivisionAsync_DivisionNotFound_ReturnsNotFound()
        {
            // Arrange
            var divName = "NONEXISTENT";

            _serviceMock.DeleteDivisionAsync(divName).Returns(false);

            // Act
            var result = await _controller.DeleteDivisionAsync(divName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal($"Division with name '{divName}' not found", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteDivisionAsync_WithFKReferences_ReturnsBadRequest()
        {
            // Arrange
            var divName = "VSD";

            _serviceMock.DeleteDivisionAsync(divName)
                .Throws(new InvalidOperationException("Unable to delete the division name as it is already in use."));

            // Act
            var result = await _controller.DeleteDivisionAsync(divName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteDivisionAsync_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            var divName = "VSD";

            _serviceMock.DeleteDivisionAsync(divName)
                .Throws(new Exception("Database connection failed"));

            // Act
            var result = await _controller.DeleteDivisionAsync(divName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion
    }
}
