using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.Costbook.Application.UnitTests.Services.StaffServiceTest
{
    public class StaffServiceTests
    {
        private readonly IStaffRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly StaffService _staffService;

        public StaffServiceTests()
        {
            _mockRepository = Substitute.For<IStaffRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _staffService = new StaffService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsStaffDtos()
        {
            // Arrange
            var staff = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "John Doe" },
                new Staff { Mnumber = "M002", Name = "Jane Smith" }
            };
            var staffDtos = new List<StaffDto>
            {
                new StaffDto { Mnumber = "M001", Name = "John Doe" },
                new StaffDto { Mnumber = "M002", Name = "Jane Smith" }
            };

            _mockRepository.GetAllStaffAsync().Returns(staff);
            _mockMapper.Map<List<StaffDto>>(staff).Returns(staffDtos);

            // Act
            var result = await _staffService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("M001", result[0].Mnumber);
            Assert.Equal("John Doe", result[0].Name);
            Assert.Equal("M002", result[1].Mnumber);
            await _mockRepository.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<List<StaffDto>>(staff);
        }

        [Fact]
        public async Task GetAllStaffAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var staff = new List<Staff>();
            var staffDtos = new List<StaffDto>();

            _mockRepository.GetAllStaffAsync().Returns(staff);
            _mockMapper.Map<List<StaffDto>>(staff).Returns(staffDtos);

            // Act
            var result = await _staffService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockRepository.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<List<StaffDto>>(staff);
        }

        [Fact]
        public async Task GetAllStaffAsync_SingleResult_ReturnsSingleItem()
        {
            // Arrange
            var staff = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "John Doe", Dt2number = "DT001" }
            };
            var staffDtos = new List<StaffDto>
            {
                new StaffDto { Mnumber = "M001", Name = "John Doe", Dt2number = "DT001" }
            };

            _mockRepository.GetAllStaffAsync().Returns(staff);
            _mockMapper.Map<List<StaffDto>>(staff).Returns(staffDtos);

            // Act
            var result = await _staffService.GetAllStaffAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("M001", result[0].Mnumber);
            Assert.Equal("DT001", result[0].Dt2number);
            await _mockRepository.Received(1).GetAllStaffAsync();
        }
    }
}
