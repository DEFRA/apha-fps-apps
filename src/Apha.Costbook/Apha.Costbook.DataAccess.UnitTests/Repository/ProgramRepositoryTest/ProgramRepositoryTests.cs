using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.ProgramRepositoryTest
{
    public class ProgramRepositoryTests
    {
        private static ProgramRepository CreateRepository(IEnumerable<Program> programs)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            mockContext.Setup(x => x.Set<Program>()).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProgramRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsAllPrograms()
        {
            // Arrange
            var programs = new List<Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Program A" },
                new() { ProgramNo = "P002", ProgramName = "Program B" },
                new() { ProgramNo = "P003", ProgramName = "Program C" }
            };
            var repo = CreateRepository(programs);

            // Act
            var result = await repo.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsEmptyList_WhenNoPrograms()
        {
            // Arrange
            var repo = CreateRepository(new List<Program>());

            // Act
            var result = await repo.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsProgramsOrderedByProgramNo()
        {
            // Arrange
            var programs = new List<Program>
            {
                new() { ProgramNo = "P003", ProgramName = "Program C" },
                new() { ProgramNo = "P001", ProgramName = "Program A" },
                new() { ProgramNo = "P002", ProgramName = "Program B" }
            };
            var repo = CreateRepository(programs);

            // Act
            var result = await repo.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("P001", result[0].ProgramNo);
            Assert.Equal("P002", result[1].ProgramNo);
            Assert.Equal("P003", result[2].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsCorrectProgramProperties()
        {
            // Arrange
            var programs = new List<Program>
            {
                new()
                {
                    ProgramNo = "P001",
                    ProgramName = "Surveillance",
                    Directorate = "APHA",
                    Customer = "DEFRA",
                    Manager = "Manager A"
                }
            };
            var repo = CreateRepository(programs);

            // Act
            var result = await repo.GetAllProgramsAsync();

            // Assert
            Assert.Single(result);
            var program = result[0];
            Assert.Equal("P001", program.ProgramNo);
            Assert.Equal("Surveillance", program.ProgramName);
            Assert.Equal("APHA", program.Directorate);
            Assert.Equal("DEFRA", program.Customer);
            Assert.Equal("Manager A", program.Manager);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsSingleProgram_WhenOnlyOneExists()
        {
            // Arrange
            var programs = new List<Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Only Program" }
            };
            var repo = CreateRepository(programs);

            // Act
            var result = await repo.GetAllProgramsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("P001", result[0].ProgramNo);
        }
    }
}
