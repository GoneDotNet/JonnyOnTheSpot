using WeatherApp.Models;

namespace WeatherApp.Services;

public interface IWeatherService
{
    Task<WeatherData> GetWeatherAsync(double latitude, double longitude);
    Task<LocationWeatherSummary> GetWeatherSummaryAsync(SavedLocation location);
}
