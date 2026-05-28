using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.AnimalMasterControllerTest
{
    public class AnimalMasterControllerTests
    {
        private readonly IAnimalService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly AnimalMasterController _controller;

        public AnimalMasterControllerTests()
        {
            _serviceMock = Substitute.For<IAnimalService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new AnimalMasterController(_serviceMock, _mapperMock);
        }

        private static AnimalDto BuildDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static AnimalMasterReq BuildReq(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static AnimalRes BuildRes(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AnimalMasterController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AnimalMasterController(_serviceMock, null!));
        }

        #endregion

        #region Access / Authorization Attribute Tests

        [Fact]
        public void Controller_HasAuthorizeAttribute_WithExpectedRoles()
        {
            var attrs = typeof(AnimalMasterController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotEmpty(attrs);
            var auth = (AuthorizeAttribute)attrs[0];
            Assert.Contains("API-FPSAdmin", auth.Roles);
        }

        [Fact]
        public void GetAllAnimalsAsync_HasHttpGetAttribute()
        {
            var method = typeof(AnimalMasterController).GetMethod(nameof(AnimalMasterController.GetAllAnimalsAsync));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetAllAnimalsPagedAsync_HasHttpGetAttribute_WithPagedRoute()
        {
            var method = typeof(AnimalMasterController).GetMethod(nameof(AnimalMasterController.GetAllAnimalsPagedAsync));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true)
                .Cast<HttpGetAttribute>().FirstOrDefault();
            Assert.NotNull(attr);
            Assert.Equal("paged", attr!.Template);
        }

        [Fact]
        public void CreateAnimal_HasHttpPostAttribute()
        {
            var method = typeof(AnimalMasterController).GetMethod(nameof(AnimalMasterController.CreateAnimal));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void UpdateAnimal_HasHttpPutAttribute()
        {
            var method = typeof(AnimalMasterController).GetMethod(nameof(AnimalMasterController.UpdateAnimal));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPutAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void DeleteAnimal_HasHttpDeleteAttribute()
        {
            var method = typeof(AnimalMasterController).GetMethod(nameof(AnimalMasterController.DeleteAnimal));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), true);
            Assert.NotEmpty(attr);
        }

        #endregion

        #region GetAllAnimalsAsync Tests

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsOk_WithMappedList()
        {
            var dtos = new List<AnimalDto> { BuildDto() };
            var resList = new List<AnimalRes> { BuildRes() };

            _serviceMock.GetAllAnimalsAsync().Returns(dtos);
            _mapperMock.Map<List<AnimalRes>>(dtos).Returns(resList);

            var result = await _controller.GetAllAnimalsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, ok.Value);
            await _serviceMock.Received(1).GetAllAnimalsAsync();
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsEmptyList_WhenNoAnimals()
        {
            _serviceMock.GetAllAnimalsAsync().Returns(new List<AnimalDto>());
            _mapperMock.Map<List<AnimalRes>>(Arg.Any<IEnumerable<AnimalDto>>())
                .Returns(new List<AnimalRes>());

            var result = await _controller.GetAllAnimalsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new List<AnimalRes>(), ok.Value);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ThrowsException_WhenServiceThrows()
        {
            _serviceMock.GetAllAnimalsAsync().ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllAnimalsAsync());
        }

        #endregion

        #region GetAllAnimalsPagedAsync Tests

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsOk_WithPagedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PaginatedResult<AnimalDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginationRes<AnimalRes>
            {
                Data = [BuildRes()],
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetAllAnimalsAsync(query).Returns(paged);
            _mapperMock.Map<PaginationRes<AnimalRes>>(paged).Returns(expected);

            var result = await _controller.GetAllAnimalsPagedAsync(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, ok.Value);
            await _serviceMock.Received(1).GetAllAnimalsAsync(query);
        }

        #endregion

        #region GetAnimalByIdAsync Tests

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsOk_WhenFound()
        {
            var dto = BuildDto();
            var res = BuildRes();

            _serviceMock.GetAnimalByIdAsync("CATTLE").Returns(dto);
            _mapperMock.Map<AnimalRes>(dto).Returns(res);

            var result = await _controller.GetAnimalByIdAsync("CATTLE");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenNotFound()
        {
            _serviceMock.GetAnimalByIdAsync("NOTEXIST").Returns((AnimalDto?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetAnimalByIdAsync("NOTEXIST"));
        }

        #endregion

        #region CreateAnimal Tests

        [Fact]
        public async Task CreateAnimal_ReturnsOk_WhenSuccessful()
        {
            var req = BuildReq();
            var dto = BuildDto();
            var addedDto = BuildDto();
            var res = BuildRes();

            _mapperMock.Map<AnimalDto>(req).Returns(dto);
            _serviceMock.AddAnimalAsync(dto).Returns(addedDto);
            _mapperMock.Map<AnimalRes>(addedDto).Returns(res);

            var result = await _controller.CreateAnimal(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
            await _serviceMock.Received(1).AddAnimalAsync(dto);
        }

        [Fact]
        public async Task CreateAnimal_ThrowsException_WhenServiceThrows()
        {
            var req = BuildReq();
            var dto = BuildDto();
            _mapperMock.Map<AnimalDto>(req).Returns(dto);
            _serviceMock.AddAnimalAsync(dto).ThrowsAsync(new ArgumentException("Animal type is required."));

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateAnimal(req));
        }

        #endregion

        #region UpdateAnimal Tests

        [Fact]
        public async Task UpdateAnimal_ReturnsOk_WhenSuccessful()
        {
            var req = BuildReq();
            var dto = BuildDto();
            var updatedDto = BuildDto();
            var res = BuildRes();

            _mapperMock.Map<AnimalDto>(req).Returns(dto);
            _serviceMock.UpdateAnimalAsync(dto).Returns(updatedDto);
            _mapperMock.Map<AnimalRes>(updatedDto).Returns(res);

            var result = await _controller.UpdateAnimal(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, ok.Value);
            await _serviceMock.Received(1).UpdateAnimalAsync(dto);
        }

        [Fact]
        public async Task UpdateAnimal_ThrowsKeyNotFoundException_WhenNotFound()
        {
            var req = BuildReq("NOTEXIST");
            var dto = new AnimalDto { AnimalType = "NOTEXIST" };
            _mapperMock.Map<AnimalDto>(req).Returns(dto);
            _serviceMock.UpdateAnimalAsync(dto).ThrowsAsync(
                new KeyNotFoundException("Animal 'NOTEXIST' not found."));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateAnimal(req));
        }

        #endregion

        #region DeleteAnimal Tests

        [Fact]
        public async Task DeleteAnimal_ThrowsArgumentException_WhenAnimalTypeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAnimal(""));
        }

        [Fact]
        public async Task DeleteAnimal_ThrowsArgumentException_WhenAnimalTypeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAnimal("   "));
        }

        [Fact]
        public async Task DeleteAnimal_ReturnsOk_WhenDeleted()
        {
            _serviceMock.DeleteAnimalAsync("CATTLE").Returns(true);

            var result = await _controller.DeleteAnimal("CATTLE");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task DeleteAnimal_ThrowsArgumentException_WhenNotFound()
        {
            _serviceMock.DeleteAnimalAsync("NOTEXIST").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAnimal("NOTEXIST"));
        }

        #endregion
    }
}
