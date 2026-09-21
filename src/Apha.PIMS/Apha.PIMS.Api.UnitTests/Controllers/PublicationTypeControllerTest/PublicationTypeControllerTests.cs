using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.PublicationTypeControllerTest
{
    public class PublicationTypeControllerTests
    {
        private readonly IPublicationTypeService _service;
        private readonly IMapper _mapper;
        private readonly PublicationTypeController _controller;

        public PublicationTypeControllerTests()
        {
            _service = Substitute.For<IPublicationTypeService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new PublicationTypeController(_service, _mapper);
        }

        [Fact]
        public async Task GetPublicationTypeByCode_WithValidType_ReturnsOkResult_WithMappedItem()
        {
            // Arrange
            const string type = "JOU";
            var dto = new PublicationTypeDto { Type = type, Description = "Journal" };
            var res = new PublicationTypeRes { Type = type, Description = "Journal" };

            _service.GetPublicationTypeByCodeAsync(type).Returns(dto);
            _mapper.Map<PublicationTypeRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetPublicationTypeByCode(type);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _service.Received(1).GetPublicationTypeByCodeAsync(type);
            _mapper.Received(1).Map<PublicationTypeRes>(dto);
        }

        [Fact]
        public async Task GetPublicationTypeByCode_WhenServiceReturnsNull_ReturnsNullSuccessResponse()
        {
            // Arrange
            const string type = "UNK";
            _service.GetPublicationTypeByCodeAsync(type).Returns((PublicationTypeDto?)null);

            // Act
            var result = await _controller.GetPublicationTypeByCode(type);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = Assert.IsType<ApiResponse<PublicationTypeRes>>(jsonResult.Value);
            Assert.True(value.Success);
            Assert.Null(value.Data);
            Assert.NotNull(value.Meta);
            Assert.False(string.IsNullOrWhiteSpace(value.Meta.CorrelationId));
            _mapper.DidNotReceive().Map<PublicationTypeRes>(Arg.Any<PublicationTypeDto>());
        }

        [Fact]
        public async Task GetPublicationTypeByCode_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string type = "JOU";
            _service.GetPublicationTypeByCodeAsync(type).Throws(new Exception("Database error"));

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPublicationTypeByCode(type));
            await _service.Received(1).GetPublicationTypeByCodeAsync(type);
            _mapper.DidNotReceive().Map<PublicationTypeRes>(Arg.Any<PublicationTypeDto>());
        }
    }
}
