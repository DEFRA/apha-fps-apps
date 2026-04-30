using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using AutoMapper;
using NSubstitute;

namespace Apha.Costbook.Application.UnitTests.Services.ProgramServiceTest
{
    public class ProgramServiceTests
    {
        private readonly IProgramRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProgramService _programService;

        public ProgramServiceTests()
        {
            _mockRepository = Substitute.For<IProgramRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _programService = new ProgramService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsProgramDtos()
        {
            // Arrange
            var programs = new List<Program>
            {
                new Program { ProgramNo = "PROG001", ProgramName = "Program A" },
                new Program { ProgramNo = "PROG002", ProgramName = "Program B" }
            };
            var programDtos = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "PROG001", ProgramName = "Program A" },
                new ProgramDto { ProgramNo = "PROG002", ProgramName = "Program B" }
            };

            _mockRepository.GetAllProgramsAsync().Returns(programs);
            _mockMapper.Map<List<ProgramDto>>(programs).Returns(programDtos);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("PROG001", result[0].ProgramNo);
            Assert.Equal("PROG002", result[1].ProgramNo);
            await _mockRepository.Received(1).GetAllProgramsAsync();
            _mockMapper.Received(1).Map<List<ProgramDto>>(programs);
        }

        [Fact]
        public async Task GetAllProgramsAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var programs = new List<Program>();
            var programDtos = new List<ProgramDto>();

            _mockRepository.GetAllProgramsAsync().Returns(programs);
            _mockMapper.Map<List<ProgramDto>>(programs).Returns(programDtos);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockRepository.Received(1).GetAllProgramsAsync();
            _mockMapper.Received(1).Map<List<ProgramDto>>(programs);
        }

        [Fact]
        public async Task GetAllProgramsAsync_SingleResult_ReturnsSingleItem()
        {
            // Arrange
            var programs = new List<Program>
            {
                new Program { ProgramNo = "PROG001", ProgramName = "Program A", Customer = "Customer X" }
            };
            var programDtos = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "PROG001", ProgramName = "Program A", Customer = "Customer X" }
            };

            _mockRepository.GetAllProgramsAsync().Returns(programs);
            _mockMapper.Map<List<ProgramDto>>(programs).Returns(programDtos);

            // Act
            var result = await _programService.GetAllProgramsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("PROG001", result[0].ProgramNo);
            Assert.Equal("Customer X", result[0].Customer);
            await _mockRepository.Received(1).GetAllProgramsAsync();
        }
    }
}
