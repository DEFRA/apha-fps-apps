using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.PIMS.Api.UnitTests.Controllers.SettingControllerTest
{
    public class SettingControllerTests
    {
        private readonly ISettingService _service;
        private readonly IMapper _mapper;
        private readonly SettingController _controller;

        public SettingControllerTests()
        {
            _service = Substitute.For<ISettingService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SettingController(_service, _mapper);
        }

        #region GetAllSettings

        [Fact]
        public async Task GetAllSettings_WhenServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var settingDtos = new List<SettingDto>
            {
                new SettingDto { Id = "setting1", Setting = "Test1", UserUpdateable = true },
                new SettingDto { Id = "setting2", Setting = "Test2", UserUpdateable = false }
            };
            var settingRes = new List<SettingRes>
            {
                new SettingRes { Id = "setting1", Setting = "Test1", UserUpdateable = true },
                new SettingRes { Id = "setting2", Setting = "Test2", UserUpdateable = false }
            };

            _service.GetAllSettingsAsync().Returns(settingDtos);
            _mapper.Map<List<SettingRes>>(settingDtos).Returns(settingRes);

            // Act
            var result = await _controller.GetAllSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedData = Assert.IsType<List<SettingRes>>(okResult.Value);
            Assert.Equal(2, returnedData.Count);

            await _service.Received(1).GetAllSettingsAsync();
            _mapper.Received(1).Map<List<SettingRes>>(settingDtos);
        }

        [Fact]
        public async Task GetAllSettings_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<SettingDto>();
            _service.GetAllSettingsAsync().Returns(emptyList);
            _mapper.Map<List<SettingRes>>(emptyList).Returns(new List<SettingRes>());

            // Act
            var result = await _controller.GetAllSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<List<SettingRes>>(okResult.Value);
            Assert.Empty(returnedData);
        }

        [Fact]
        public async Task GetAllSettings_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllSettingsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllSettings());
            await _service.Received(1).GetAllSettingsAsync();
            _mapper.DidNotReceive().Map<List<SettingRes>>(Arg.Any<List<SettingDto>>());
        }

        #endregion

        #region GetAllUserUpdateableSettings

        [Fact]
        public async Task GetAllUserUpdateableSettings_WhenServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var settingDtos = new List<SettingDto>
            {
                new SettingDto { Id = "updateable1", Setting = "UpdateTest", UserUpdateable = true }
            };
            var settingRes = new List<SettingRes>
            {
                new SettingRes { Id = "updateable1", Setting = "UpdateTest", UserUpdateable = true }
            };

            _service.GetAllUserUpdateableSettingsAsync().Returns(settingDtos);
            _mapper.Map<List<SettingRes>>(settingDtos).Returns(settingRes);

            // Act
            var result = await _controller.GetAllUserUpdateableSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<List<SettingRes>>(okResult.Value);
            Assert.Single(returnedData);

            await _service.Received(1).GetAllUserUpdateableSettingsAsync();
        }

        [Fact]
        public async Task GetAllUserUpdateableSettings_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<SettingDto>();
            _service.GetAllUserUpdateableSettingsAsync().Returns(emptyList);
            _mapper.Map<List<SettingRes>>(emptyList).Returns(new List<SettingRes>());

            // Act
            var result = await _controller.GetAllUserUpdateableSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<List<SettingRes>>(okResult.Value);
            Assert.Empty(returnedData);
        }

        [Fact]
        public async Task GetAllUserUpdateableSettings_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllUserUpdateableSettingsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllUserUpdateableSettings());
            await _service.Received(1).GetAllUserUpdateableSettingsAsync();
        }

        #endregion

        #region GetSettingById

        [Fact]
        public async Task GetSettingById_WhenSettingExists_ReturnsOkWithMappedResponse()
        {
            // Arrange
            var id = "setting1";
            var settingDto = new SettingDto { Id = id, Setting = "TestSetting", UserUpdateable = true };
            var settingRes = new SettingRes { Id = id, Setting = "TestSetting", UserUpdateable = true };

            _service.GetSettingByIdAsync(id).Returns(settingDto);
            _mapper.Map<SettingRes>(settingDto).Returns(settingRes);

            // Act
            var result = await _controller.GetSettingById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedData = Assert.IsType<SettingRes>(okResult.Value);
            Assert.Equal(id, returnedData.Id);
            Assert.Equal("TestSetting", returnedData.Setting);

            await _service.Received(1).GetSettingByIdAsync(id);
            _mapper.Received(1).Map<SettingRes>(settingDto);
        }

        [Fact]
        public async Task GetSettingById_WhenSettingDoesNotExist_ReturnsJsonSuccessResponseWithNullData()
        {
            // Arrange
            var id = "nonexistent";
            _service.GetSettingByIdAsync(id).Returns((SettingDto?)null);

            // Act
            var result = await _controller.GetSettingById(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var apiResponse = Assert.IsType<Apha.Common.Contracts.ApiResponse<SettingRes>>(jsonResult.Value);

            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.NotNull(apiResponse.Meta);
            Assert.NotNull(apiResponse.Meta.CorrelationId);
            Assert.True(apiResponse.Meta.TimestampUtc > DateTime.MinValue);

            await _service.Received(1).GetSettingByIdAsync(id);
            _mapper.DidNotReceive().Map<SettingRes>(Arg.Any<SettingDto>());
        }

        [Fact]
        public async Task GetSettingById_WithUrlEncodedId_DecodesAndRetrievesSetting()
        {
            // Arrange
            var originalId = "setting/with spaces";
            var encodedId = System.Web.HttpUtility.UrlEncode(originalId);
            var settingDto = new SettingDto { Id = originalId, Setting = "Encoded", UserUpdateable = false };
            var settingRes = new SettingRes { Id = originalId, Setting = "Encoded", UserUpdateable = false };

            _service.GetSettingByIdAsync(originalId).Returns(settingDto);
            _mapper.Map<SettingRes>(settingDto).Returns(settingRes);

            // Act
            var result = await _controller.GetSettingById(encodedId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<SettingRes>(okResult.Value);
            Assert.Equal(originalId, returnedData.Id);

            await _service.Received(1).GetSettingByIdAsync(originalId);
        }

        [Fact]
        public async Task GetSettingById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var id = "setting1";
            _service.GetSettingByIdAsync(id).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetSettingById(id));
            await _service.Received(1).GetSettingByIdAsync(id);
            _mapper.DidNotReceive().Map<SettingRes>(Arg.Any<SettingDto>());
        }

        #endregion

        #region UpdateSetting

        [Fact]
        public async Task UpdateSetting_WhenValidRequest_ReturnsOkWithUpdatedResponse()
        {
            // Arrange
            var id = "setting1";
            var request = new SettingReq { Setting = "Updated", UserUpdateable = true };
            var settingDto = new SettingDto { Id = id, Setting = "Updated", UserUpdateable = true };
            var settingRes = new SettingRes { Id = id, Setting = "Updated", UserUpdateable = true };

            _mapper.Map<SettingDto>(request).Returns(settingDto);
            _service.UpdateSettingAsync(Arg.Any<SettingDto>()).Returns(settingDto);
            _mapper.Map<SettingRes>(settingDto).Returns(settingRes);

            // Act
            var result = await _controller.UpdateSetting(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedData = Assert.IsType<SettingRes>(okResult.Value);
            Assert.Equal(id, returnedData.Id);
            Assert.Equal("Updated", returnedData.Setting);

            await _service.Received(1).UpdateSettingAsync(Arg.Is<SettingDto>(dto => dto.Id == id));
            _mapper.Received(1).Map<SettingDto>(request);
        }

        [Fact]
        public async Task UpdateSetting_WithUrlEncodedId_DecodesAndUpdates()
        {
            // Arrange
            var originalId = "setting/encoded";
            var encodedId = System.Web.HttpUtility.UrlEncode(originalId);
            var request = new SettingReq { Setting = "NewValue", UserUpdateable = false };
            var settingDto = new SettingDto { Id = originalId, Setting = "NewValue", UserUpdateable = false };
            var settingRes = new SettingRes { Id = originalId, Setting = "NewValue", UserUpdateable = false };

            _mapper.Map<SettingDto>(request).Returns(settingDto);
            _service.UpdateSettingAsync(Arg.Any<SettingDto>()).Returns(settingDto);
            _mapper.Map<SettingRes>(settingDto).Returns(settingRes);

            // Act
            var result = await _controller.UpdateSetting(encodedId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<SettingRes>(okResult.Value);
            Assert.Equal(originalId, returnedData.Id);

            await _service.Received(1).UpdateSettingAsync(Arg.Is<SettingDto>(dto => dto.Id == originalId));
        }

        [Fact]
        public async Task UpdateSetting_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var id = "setting1";
            var request = new SettingReq { Setting = "Test" };
            var settingDto = new SettingDto { Id = id, Setting = "Test" };

            _mapper.Map<SettingDto>(request).Returns(settingDto);
            _service.UpdateSettingAsync(Arg.Any<SettingDto>()).Throws(new Exception("Update failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateSetting(id, request));
            await _service.Received(1).UpdateSettingAsync(Arg.Any<SettingDto>());
        }

        #endregion
    }
}
