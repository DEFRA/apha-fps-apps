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

namespace Apha.FPS.Api.UnitTests.Controller.MonthlyOutputControllerTest
{
    public class MonthlyOutputControllerTests
    {
        private readonly IMonthlyOutputService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly MonthlyOutputController _controller;

        public MonthlyOutputControllerTests()
        {
            _serviceMock = Substitute.For<IMonthlyOutputService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new MonthlyOutputController(_serviceMock, _mapperMock);
        }

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };

        private static PaginatedResult<MonthlyOutputDto> MakeResult(int count) =>
            new(Enumerable.Range(1, count).Select(i => new MonthlyOutputDto { Buyer = "AH0033", TestCode = $"TC{i:D2}" }).ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });

        [Fact]
        public async Task GetByProjectAsync_HappyPath_ReturnsOk()
        {
            var query = DefaultQuery(); var projectCode = "AH0033";
            var serviceResult = MakeResult(2);
            var mappedResult  = new PaginationRes<MonthlyOutputRes> { Data = new List<MonthlyOutputRes> { new() { Buyer = "AH0033", TestCode = "TC01" }, new() { Buyer = "AH0033", TestCode = "TC02" } } };
            _serviceMock.GetByProjectAsync(query, projectCode).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputRes>>(serviceResult).Returns(mappedResult);
            var result = await _controller.GetByProjectAsync(query, projectCode);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ok.Value);
        }

        [Fact]
        public async Task GetByProjectAsync_EmptyData_ReturnsOkWithEmptyList()
        {
            var query = DefaultQuery(); var projectCode = "AH0033";
            var serviceResult = MakeResult(0);
            var mappedResult  = new PaginationRes<MonthlyOutputRes> { Data = new List<MonthlyOutputRes>() };
            _serviceMock.GetByProjectAsync(query, projectCode).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputRes>>(serviceResult).Returns(mappedResult);
            var result = await _controller.GetByProjectAsync(query, projectCode);
            var value = Assert.IsType<PaginationRes<MonthlyOutputRes>>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Empty(value.Data);
        }

        [Theory]
        [InlineData(null)][InlineData("")][InlineData("   ")]
        public async Task GetByProjectAsync_WhenProjectCodeIsNullOrWhitespace_ThrowsArgumentException(string? projectCode)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetByProjectAsync(DefaultQuery(), projectCode!));
            Assert.Equal("projectCode is required.", ex.Message);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_HappyPath_ReturnsOk()
        {
            _serviceMock.GetTotalActualByProjectAsync("AH0033").Returns(10.0);
            var result = await _controller.GetTotalActualByProjectAsync("AH0033");
            Assert.Equal(10.0, Assert.IsType<OkObjectResult>(result).Value);
        }

        [Theory]
        [InlineData(null)][InlineData("")][InlineData("   ")]
        public async Task GetTotalActualByProjectAsync_WhenProjectCodeIsNullOrWhitespace_ThrowsArgumentException(string? projectCode)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetTotalActualByProjectAsync(projectCode!));
            Assert.Equal("projectCode is required.", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithValidRequest_ReturnsOk()
        {
            var req = new MonthlyOutputReq { Buyer = "AH0033", TestCode = "TC01", Month = 1, WorkGroup = "WG1" };
            _serviceMock.DeleteAsync(req.Buyer, req.TestCode, req.Month, req.WorkGroup).Returns(true);
            Assert.IsType<OkObjectResult>(await _controller.DeleteAsync(req));
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
        {
            var req = new MonthlyOutputReq { Buyer = "XX", TestCode = "XX", Month = 1, WorkGroup = "XX" };
            _serviceMock.DeleteAsync(req.Buyer, req.TestCode, req.Month, req.WorkGroup).Returns(false);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteAsync(req));
        }

        [Fact]
        public async Task DeleteAsync_WhenRequestIsNull_ThrowsArgumentException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(null!));
            Assert.Equal("Request body is required.", ex.Message);
        }

        [Theory]
        [InlineData("", "TC01", "WG1")][InlineData("AH0033", "", "WG1")][InlineData("AH0033", "TC01", "")]
        public async Task DeleteAsync_WhenRequiredFieldsMissing_ThrowsArgumentException(string buyer, string testCode, string workGroup)
        {
            var req = new MonthlyOutputReq { Buyer = buyer, TestCode = testCode, Month = 1, WorkGroup = workGroup };
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(req));
            Assert.Equal("Buyer, TestCode and WorkGroup are required.", ex.Message);
        }
    }
}