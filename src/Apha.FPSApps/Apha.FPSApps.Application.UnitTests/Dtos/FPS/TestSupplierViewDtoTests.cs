using Apha.FPSApps.Application.Dtos.FPS;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class TestSupplierViewDtoTests
    {
        [Fact]
        public void TestSupplierViewDto_DefaultValues_AreCorrect()
        {
            var dto = new TestSupplierViewDto();

            Assert.Null(dto.ProjectManager);
            Assert.Null(dto.NoTests);
            Assert.Null(dto.TestPrice);
            Assert.Equal(0m, dto.TestCost);
            Assert.Null(dto.ProjectStatus);
        }

        [Fact]
        public void TestSupplierViewDto_RequiredProperties_CanBeSet()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TEST001",
                JobCode = "JOB001"
            };

            Assert.Equal("TEST001", dto.TestCode);
            Assert.Equal("JOB001", dto.JobCode);
        }

        [Fact]
        public void TestSupplierViewDto_AllProperties_CanBeSetAndRetrieved()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TEST002",
                JobCode = "JOB002",
                ProjectManager = "Jane Doe",
                NoTests = 5.0,
                TestPrice = 99.99m,
                TestCost = 499.95m,
                ProjectStatus = "Approved"
            };

            Assert.Equal("TEST002", dto.TestCode);
            Assert.Equal("JOB002", dto.JobCode);
            Assert.Equal("Jane Doe", dto.ProjectManager);
            Assert.Equal(5.0, dto.NoTests);
            Assert.Equal(99.99m, dto.TestPrice);
            Assert.Equal(499.95m, dto.TestCost);
            Assert.Equal("Approved", dto.ProjectStatus);
        }

        [Fact]
        public void TestSupplierViewDto_NullableProperties_AcceptNull()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TEST003",
                JobCode = "JOB003",
                ProjectManager = null,
                NoTests = null,
                TestPrice = null,
                ProjectStatus = null
            };

            Assert.Null(dto.ProjectManager);
            Assert.Null(dto.NoTests);
            Assert.Null(dto.TestPrice);
            Assert.Null(dto.ProjectStatus);
        }

        [Fact]
        public void TestSupplierViewDto_TestCost_CanBeSetToZero()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TEST004",
                JobCode = "JOB004",
                TestCost = 0m
            };

            Assert.Equal(0m, dto.TestCost);
        }

        [Fact]
        public void TestSupplierViewDto_ProjectStatus_AcceptsRejected()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TEST005",
                JobCode = "JOB005",
                ProjectStatus = "Rejected"
            };

            Assert.Equal("Rejected", dto.ProjectStatus);
        }
    }
}
