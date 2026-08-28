using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.MonthlyOutputControllerTest
{
    public class MonthlyOutputControllerTests
    {
        private readonly IMonthlyOutputService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ICurrentUserContext _currentUserContextMock;
        private readonly MonthlyOutputController _controller;

        public MonthlyOutputControllerTests()
        {
            _serviceMock = Substitute.For<IMonthlyOutputService>();
            _mapperMock = Substitute.For<IMapper>();
            _currentUserContextMock = Substitute.For<ICurrentUserContext>();
            _controller = new MonthlyOutputController(_serviceMock, _mapperMock, _currentUserContextMock);
        }

        #region SearchAsync

        [Fact]
        public async Task SearchAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>
            {
                Data = new List<MonthlyOutputLogDto> { new() { TestCode = "TC1", Buyer = "BuyerA" } },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>
            {
                Data = new List<MonthlyOutputLogRes> { new() { TestCode = "TC1", Buyer = "BuyerA" } }
            };

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToService()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var dateImported = new DateTime(2024, 1, 15);
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto> { Data = new List<MonthlyOutputLogDto>() };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>();

            _serviceMock.GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I")
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I");
        }

        [Fact]
        public async Task SearchAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>
            {
                Data = new List<MonthlyOutputLogDto>(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>
            {
                Data = new List<MonthlyOutputLogRes>()
            };

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<MonthlyOutputLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task SearchAsync_MapsServiceResultToResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>();
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>();

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mapperMock.Received(1).Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task SearchAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();

            _serviceMock.GetMonthlyOutputLogAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                    Arg.Any<DateTime?>(), Arg.Any<double?>(), Arg.Any<string?>(), Arg.Any<string?>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.SearchAsync(query, null, null, null, null, null, null, null));
        }

        #endregion

        #region Live and Staging Endpoints

        [Fact]
        public async Task GetLiveByKey_WhenItemNotFound_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns((MonthlyOutputDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetLiveByKey("TC1", "B1", 6, "WG1"));
        }

        [Fact]
        public async Task UpdateLive_WithValidRequest_ReturnsOk()
        {
            var request = new MonthlyOutputReq { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var dto = new MonthlyOutputDto { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var res = new MonthlyOutputRes { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };

            _mapperMock.Map<MonthlyOutputDto>(request).Returns(dto);
            _serviceMock.UpdateLiveAsync(dto).Returns(dto);
            _mapperMock.Map<MonthlyOutputRes>(dto).Returns(res);

            var result = await _controller.UpdateLive(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task DeleteLive_WithValidKey_ReturnsOkWithBoolean()
        {
            _serviceMock.DeleteLiveAsync("TC1", "B1", 6, "WG1").Returns(true);

            var result = await _controller.DeleteLive("TC1", "B1", 6, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task GetStagingById_WhenItemNotFound_ThrowsKeyNotFoundException()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.GetStagingByIdAsync(10, "user1").Returns((StagingMonthlyOutputDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetStagingById(10));
        }

        [Fact]
        public async Task CreateStaging_WithValidRequest_ReturnsCreatedAtAction()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var request = new StagingMonthlyOutputReq { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var dto = new StagingMonthlyOutputDto { Id = 25, TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var res = new StagingMonthlyOutputRes { Id = 25, TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };

            _mapperMock.Map<StagingMonthlyOutputDto>(request).Returns(dto);
            _serviceMock.CreateStagingAsync(dto, "user1").Returns(dto);
            _mapperMock.Map<StagingMonthlyOutputRes>(dto).Returns(res);

            var result = await _controller.CreateStaging(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(MonthlyOutputController.GetStagingById), created.ActionName);
        }

        #endregion

        #region GetLive

        [Fact]
        public async Task GetLive_HappyPath_ReturnsOkWithMappedResult()
        {
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputDto>
            {
                Data = new List<MonthlyOutputDto> { new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1" } },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputRes>
            {
                Data = new List<MonthlyOutputRes> { new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1" } }
            };

            _serviceMock.SearchLiveAsync(query, null, null, null, null).Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputRes>>(paginatedResult).Returns(mappedResult);

            var result = await _controller.GetLive(query, null, null, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ok.Value);
        }

        [Fact]
        public async Task GetLive_WithAllFilters_PassesFiltersToService()
        {
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputDto> { Data = new List<MonthlyOutputDto>() };
            var mappedResult = new PaginationRes<MonthlyOutputRes>();

            _serviceMock.SearchLiveAsync(query, "WG1", "TC1", "B1", 6.0).Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputRes>>(paginatedResult).Returns(mappedResult);

            var result = await _controller.GetLive(query, "WG1", "TC1", "B1", 6.0);

            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).SearchLiveAsync(query, "WG1", "TC1", "B1", 6.0);
        }

        [Fact]
        public async Task GetLive_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _serviceMock.SearchLiveAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<double?>())
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetLive(query, null, null, null, null));
        }

        #endregion

        #region GetLiveByKey

        [Fact]
        public async Task GetLiveByKey_WhenItemFound_ReturnsOkWithMappedResult()
        {
            var dto = new MonthlyOutputDto { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var res = new MonthlyOutputRes { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };

            _serviceMock.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns(dto);
            _mapperMock.Map<MonthlyOutputRes>(dto).Returns(res);

            var result = await _controller.GetLiveByKey("TC1", "B1", 6, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        #endregion

        #region UpdateLive

        [Fact]
        public async Task UpdateLive_ServiceThrows_PropagatesException()
        {
            var request = new MonthlyOutputReq { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var dto = new MonthlyOutputDto();

            _mapperMock.Map<MonthlyOutputDto>(request).Returns(dto);
            _serviceMock.UpdateLiveAsync(dto).ThrowsAsync(new Exception("Update error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateLive(request));
        }

        #endregion

        #region DeleteLive

        [Fact]
        public async Task DeleteLive_WhenNotFound_ReturnsOkWithFalse()
        {
            _serviceMock.DeleteLiveAsync("TC1", "B1", 6, "WG1").Returns(false);

            var result = await _controller.DeleteLive("TC1", "B1", 6, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, ok.Value);
        }

        #endregion

        #region GetStaging

        [Fact]
        public async Task GetStaging_HappyPath_ReturnsOkWithMappedResult()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<StagingMonthlyOutputDto>
            {
                Data = new List<StagingMonthlyOutputDto> { new() { Id = 1, TestCode = "TC1" } },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<StagingMonthlyOutputRes>
            {
                Data = new List<StagingMonthlyOutputRes> { new() { Id = 1, TestCode = "TC1" } }
            };

            _serviceMock.SearchStagingAsync(query, "user1", null).Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<StagingMonthlyOutputRes>>(paginatedResult).Returns(mappedResult);

            var result = await _controller.GetStaging(query, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ok.Value);
        }

        [Fact]
        public async Task GetStaging_WithPassedFilter_PassesFilterToService()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<StagingMonthlyOutputDto> { Data = new List<StagingMonthlyOutputDto>() };
            var mappedResult = new PaginationRes<StagingMonthlyOutputRes>();

            _serviceMock.SearchStagingAsync(query, "user1", true).Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<StagingMonthlyOutputRes>>(paginatedResult).Returns(mappedResult);

            var result = await _controller.GetStaging(query, true);

            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).SearchStagingAsync(query, "user1", true);
        }

        #endregion

        #region GetStagingById

        [Fact]
        public async Task GetStagingById_WhenItemFound_ReturnsOkWithMappedResult()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var dto = new StagingMonthlyOutputDto { Id = 10, TestCode = "TC1", Buyer = "B1" };
            var res = new StagingMonthlyOutputRes { Id = 10, TestCode = "TC1", Buyer = "B1" };

            _serviceMock.GetStagingByIdAsync(10, "user1").Returns(dto);
            _mapperMock.Map<StagingMonthlyOutputRes>(dto).Returns(res);

            var result = await _controller.GetStagingById(10);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        #endregion

        #region UpdateStaging

        [Fact]
        public async Task UpdateStaging_WithValidRequest_ReturnsOk()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var request = new StagingMonthlyOutputReq { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var dto = new StagingMonthlyOutputDto { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var updated = new StagingMonthlyOutputDto { Id = 5, TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var res = new StagingMonthlyOutputRes { Id = 5, TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };

            _mapperMock.Map<StagingMonthlyOutputDto>(request).Returns(dto);
            _serviceMock.UpdateStagingAsync(dto, "user1").Returns(updated);
            _mapperMock.Map<StagingMonthlyOutputRes>(updated).Returns(res);

            var result = await _controller.UpdateStaging(5, request);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            Assert.Equal(5, dto.Id);
        }

        [Fact]
        public async Task UpdateStaging_SetsIdFromRoute()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var request = new StagingMonthlyOutputReq { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var dto = new StagingMonthlyOutputDto();
            var updated = new StagingMonthlyOutputDto { Id = 99 };
            var res = new StagingMonthlyOutputRes { Id = 99 };

            _mapperMock.Map<StagingMonthlyOutputDto>(request).Returns(dto);
            _serviceMock.UpdateStagingAsync(dto, "user1").Returns(updated);
            _mapperMock.Map<StagingMonthlyOutputRes>(updated).Returns(res);

            await _controller.UpdateStaging(99, request);

            Assert.Equal(99, dto.Id);
        }

        #endregion

        #region DeleteStaging

        [Fact]
        public async Task DeleteStaging_WithValidId_ReturnsOkWithResult()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.DeleteStagingAsync(10, "user1").Returns(true);

            var result = await _controller.DeleteStaging(10);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task DeleteStaging_WhenNotFound_ReturnsOkWithFalse()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.DeleteStagingAsync(10, "user1").Returns(false);

            var result = await _controller.DeleteStaging(10);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, ok.Value);
        }

        #endregion

        #region DeleteAllStagingByUser

        [Fact]
        public async Task DeleteAllStagingByUser_WhenRecordsDeleted_ReturnsOkWithTrue()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.DeleteAllStagingByUserAsync("user1").Returns(5);

            var result = await _controller.DeleteAllStagingByUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task DeleteAllStagingByUser_WhenNoRecordsDeleted_ReturnsOkWithFalse()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.DeleteAllStagingByUserAsync("user1").Returns(0);

            var result = await _controller.DeleteAllStagingByUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, ok.Value);
        }

        #endregion

        #region DeleteFailedStagingByUser

        [Fact]
        public async Task DeleteFailedStagingByUser_WhenRecordsDeleted_ReturnsOkWithTrue()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.DeleteFailedStagingByUserAsync("user1").Returns(3);

            var result = await _controller.DeleteFailedStagingByUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task DeleteFailedStagingByUser_WhenNoRecordsDeleted_ReturnsOkWithFalse()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.DeleteFailedStagingByUserAsync("user1").Returns(0);

            var result = await _controller.DeleteFailedStagingByUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(false, ok.Value);
        }

        #endregion

        #region ImportStaging

        [Fact]
        public async Task ImportStaging_WithValidRequest_ReturnsOkWithMappedResult()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var request = new MonthlyOutputImportReq();
            var dto = new MonthlyOutputImportDto();
            var resultDto = new MonthlyOutputImportResultDto();
            var res = new MonthlyOutputImportRes();

            _mapperMock.Map<MonthlyOutputImportDto>(request).Returns(dto);
            _serviceMock.ImportStagingAsync(dto, "user1").Returns(resultDto);
            _mapperMock.Map<MonthlyOutputImportRes>(resultDto).Returns(res);

            var result = await _controller.ImportStaging(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task ImportStaging_ServiceThrows_PropagatesException()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var request = new MonthlyOutputImportReq();
            var dto = new MonthlyOutputImportDto();

            _mapperMock.Map<MonthlyOutputImportDto>(request).Returns(dto);
            _serviceMock.ImportStagingAsync(dto, "user1").ThrowsAsync(new Exception("Import error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.ImportStaging(request));
        }

        #endregion

        #region ValidateStaging

        [Fact]
        public async Task ValidateStaging_ReturnsOkWithMappedResult()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var resultDto = new MonthlyOutputValidateResultDto();
            var res = new MonthlyOutputValidateRes();

            _serviceMock.ValidateStagingAsync("user1").Returns(resultDto);
            _mapperMock.Map<MonthlyOutputValidateRes>(resultDto).Returns(res);

            var result = await _controller.ValidateStaging();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task ValidateStaging_ServiceThrows_PropagatesException()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.ValidateStagingAsync("user1").ThrowsAsync(new Exception("Validate error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.ValidateStaging());
        }

        #endregion

        #region MakeLive

        [Fact]
        public async Task MakeLive_ReturnsOkWithMappedResult()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var resultDto = new MonthlyOutputMakeLiveResultDto();
            var res = new MonthlyOutputMakeLiveRes();

            _serviceMock.MakeLiveAsync("user1").Returns(resultDto);
            _mapperMock.Map<MonthlyOutputMakeLiveRes>(resultDto).Returns(res);

            var result = await _controller.MakeLive();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task MakeLive_ServiceThrows_PropagatesException()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.MakeLiveAsync("user1").ThrowsAsync(new Exception("MakeLive error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.MakeLive());
        }

        #endregion
    }
}
