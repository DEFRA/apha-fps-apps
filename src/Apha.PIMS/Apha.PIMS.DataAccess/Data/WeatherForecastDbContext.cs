using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Data
{
    public class WeatherForecastDbContext : DbContext
    {
        public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) : base(options)
        {
        }

        public virtual DbSet<WeatherForecast> Tours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WeatherForecastDbContext).Assembly);
        }
    }
}
