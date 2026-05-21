using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Data
{
    public class DivisionGradeMapTests
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
        public void DivisionGradeMap_Configure_SetsCorrectTableAndColumns()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGrade));

            Assert.NotNull(entityType);
            Assert.Equal("divisiongrade", entityType.GetTableName());
            Assert.Equal("fps", entityType.GetSchema());
        }

        [Fact]
        public void DivisionGradeMap_Configure_SetsCompositePrimaryKey()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGrade));
            var primaryKey = entityType!.FindPrimaryKey();

            Assert.NotNull(primaryKey);
            Assert.Equal(2, primaryKey.Properties.Count);
            Assert.Contains(primaryKey.Properties, p => p.Name == nameof(DivisionGrade.DivisionGradeCode));
            Assert.Contains(primaryKey.Properties, p => p.Name == nameof(DivisionGrade.FpsYear));
        }

        [Fact]
        public void DivisionGradeMap_Configure_SetsColumnNames()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGrade));

            Assert.Equal("divisiongrade", entityType!.FindProperty(nameof(DivisionGrade.DivisionGradeCode))!.GetColumnName());
            Assert.Equal("fpsyear",        entityType.FindProperty(nameof(DivisionGrade.FpsYear))!.GetColumnName());
            Assert.Equal("chargerate",     entityType.FindProperty(nameof(DivisionGrade.ChargeRate))!.GetColumnName());
            Assert.Equal("directrate",     entityType.FindProperty(nameof(DivisionGrade.DirectRate))!.GetColumnName());
            Assert.Equal("payrate",        entityType.FindProperty(nameof(DivisionGrade.PayRate))!.GetColumnName());
            Assert.Equal("npr",            entityType.FindProperty(nameof(DivisionGrade.Npr))!.GetColumnName());
            Assert.Equal("ohr",            entityType.FindProperty(nameof(DivisionGrade.Ohr))!.GetColumnName());
            Assert.Equal("division",       entityType.FindProperty(nameof(DivisionGrade.Division))!.GetColumnName());
            Assert.Equal("gradecode",      entityType.FindProperty(nameof(DivisionGrade.GradeCode))!.GetColumnName());
        }

        [Fact]
        public void DivisionGradeMap_Configure_SetsMaxLengths()
        {
            using var context = CreateDbContext();
            var model = context.Model;

            var entityType = model.FindEntityType(typeof(DivisionGrade));

            Assert.Equal(10, entityType!.FindProperty(nameof(DivisionGrade.DivisionGradeCode))!.GetMaxLength());
            Assert.Equal(10, entityType.FindProperty(nameof(DivisionGrade.Division))!.GetMaxLength());
            Assert.Equal(10, entityType.FindProperty(nameof(DivisionGrade.GradeCode))!.GetMaxLength());
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
