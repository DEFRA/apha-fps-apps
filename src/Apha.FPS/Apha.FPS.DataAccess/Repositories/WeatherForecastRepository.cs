using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Microsoft.EntityFrameworkCore.Internal;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WeatherForecastRepository : IWeatherForecastRepository
    {

        private readonly IYearContext _yearContext;
        public WeatherForecastRepository(IYearContext yearContext)
        {           
            _yearContext = yearContext;
        }

        private static readonly string[] Summaries = new[]
        {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

        public async Task<IEnumerable<WeatherForecast>> Get()
        {            
            return await GetWeatherForecasts();                    
        }

        public async Task<PagedData<WeatherForecast>> SearchWeather(PaginationParameters<object> query)
        {
            int selectedYear = Convert.ToInt32(_yearContext.Year);
            var wetherData = await GetWeatherForecasts();
            wetherData = wetherData.Where(e => e.Date.Year == selectedYear).ToList();
            if (!string.IsNullOrEmpty(query.Search))
            {
                if (DateOnly.TryParse(query.Search, out var parsedDate))
                {
                    wetherData = wetherData.Where(e => e.Date == parsedDate).ToArray();
                }
            }
            return ApplyPaging(wetherData, query.Page, query.PageSize);          
        }
       
        public async Task<PagedData<WeatherForecast>> SearchWeatherByModel(PaginationParameters<WeatherForecastCriteria> query)
        {
            int selectedYear = Convert.ToInt32(_yearContext.Year);
            var wetherData = await GetWeatherForecasts();
            wetherData = wetherData.Where(e => e.Date.Year == selectedYear).ToList();

            if (query.Filter != null)
            {
                if (DateOnly.TryParse(query.Filter.Date.ToString(), out var parsedDate)) // Fix: Convert DateOnly to string before parsing
                {
                    wetherData = wetherData.Where(e => e.Date == parsedDate).ToArray();
                }
            }

            return ApplyPaging(wetherData, query.Page, query.PageSize);
        }

        private async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            return await Task.Run(() =>
            {
                var forecasts = new List<WeatherForecast>();

                foreach (var year in new[] { 2025, 2026 })
                {
                    for (int index = 1; index <= 25; index++)
                    {
                        var date = new DateOnly(year, 1, 1).AddDays(index - 1);
                        forecasts.Add(new WeatherForecast
                        {
                            Date = date,
                            TemperatureC = Random.Shared.Next(-20, 55),
                            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                        });
                    }
                }

                return forecasts;
            });
        }

        private PagedData<T> ApplyPaging<T>(
                    IEnumerable<T> source,
                    int page,
                    int pageSize)
        {
            var list = source.ToList();
            var totalRecords = list.Count;

            var result = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagination = new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<T>(result,  pagination);
        }
    }
}
