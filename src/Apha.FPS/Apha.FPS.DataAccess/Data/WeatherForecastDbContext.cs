using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Data
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
