using WeatherApp.Models;

namespace WeatherApp.Services;

public interface IGeocodingService
{
    Task<List<GeocodingResult>> SearchAsync(string query, int maxResults = 10);
}
