using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Data
{
    public class DivisionGradeMaintenanceMapTests
    {
        private static FpsDbContext CreateDbContext()
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2025);
            mockRequestContext.Setup(x => x.UserEmailId).Returns("test@example.com");

            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new FpsDbContext(options, mockRequestContext.Object);
        }

        [Fact]
        public void DivisionGradeMaintenanceMap_Configure_SetsCorrectTableAndColumns()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGradeMaintenance));

            Assert.NotNull(entityType);
            Assert.Equal("divisiongrade", entityType.GetTableName());
            Assert.Equal("fps", entityType.GetSchema());
        }

        [Fact]
        public void DivisionGradeMaintenanceMap_Configure_SetsCompositePrimaryKey()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGradeMaintenance));
            var primaryKey = entityType!.FindPrimaryKey();

            Assert.NotNull(primaryKey);
            Assert.Equal(2, primaryKey.Properties.Count);
            Assert.Contains(primaryKey.Properties, p => p.Name == nameof(DivisionGradeMaintenance.DivisionGradeCode));
            Assert.Contains(primaryKey.Properties, p => p.Name == nameof(DivisionGradeMaintenance.FpsYear));
        }

        [Fact]
        public void DivisionGradeMaintenanceMap_Configure_SetsColumnNames()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGradeMaintenance));

            Assert.Equal("divisiongrade", entityType!.FindProperty(nameof(DivisionGradeMaintenance.DivisionGradeCode))!.GetColumnName());
            Assert.Equal("fpsyear",        entityType.FindProperty(nameof(DivisionGradeMaintenance.FpsYear))!.GetColumnName());
            Assert.Equal("chargerate",     entityType.FindProperty(nameof(DivisionGradeMaintenance.ChargeRate))!.GetColumnName());
            Assert.Equal("directrate",     entityType.FindProperty(nameof(DivisionGradeMaintenance.DirectRate))!.GetColumnName());
            Assert.Equal("payrate",        entityType.FindProperty(nameof(DivisionGradeMaintenance.PayRate))!.GetColumnName());
            Assert.Equal("npr",            entityType.FindProperty(nameof(DivisionGradeMaintenance.Npr))!.GetColumnName());
            Assert.Equal("ohr",            entityType.FindProperty(nameof(DivisionGradeMaintenance.Ohr))!.GetColumnName());
            Assert.Equal("division",       entityType.FindProperty(nameof(DivisionGradeMaintenance.Division))!.GetColumnName());
            Assert.Equal("gradecode",      entityType.FindProperty(nameof(DivisionGradeMaintenance.GradeCode))!.GetColumnName());
        }

        [Fact]
        public void DivisionGradeMaintenanceMap_Configure_SetsMaxLengths()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGradeMaintenance));

            Assert.Equal(10, entityType!.FindProperty(nameof(DivisionGradeMaintenance.DivisionGradeCode))!.GetMaxLength());
            Assert.Equal(10, entityType.FindProperty(nameof(DivisionGradeMaintenance.Division))!.GetMaxLength());
            Assert.Equal(10, entityType.FindProperty(nameof(DivisionGradeMaintenance.GradeCode))!.GetMaxLength());
        }

        [Fact]
        public void FpsDbContext_FilterFpsYear_ReturnsCorrectYear()
        {
            using var context = CreateDbContext();
            Assert.Equal(2025, context.FilterFpsYear);
        }

        [Fact]
        public void FpsDbContext_DivisionGrades_DbSetIsNotNull()
        {
            using var context = CreateDbContext();
            Assert.NotNull(context.DivisionGrades);
        }

        [Fact]
        public void FpsDbContext_Grades_DbSetIsNotNull()
        {
            using var context = CreateDbContext();
            Assert.NotNull(context.Grades);
        }
    }
}
