using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class OpenMeteoGeocodingService : IGeocodingService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenMeteoGeocodingService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<GeocodingResult>> SearchAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var client = _httpClientFactory.CreateClient("OpenMeteoGeocoding");
        var url = $"/v1/search?name={Uri.EscapeDataString(query)}&count={maxResults}&language=en";

        var json = await client.GetStringAsync(url);
        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("results", out var results))
            return [];

        var items = new List<GeocodingResult>();
        foreach (var result in results.EnumerateArray())
        {
            items.Add(new GeocodingResult
            {
                Name = result.GetProperty("name").GetString() ?? "",
                Admin1 = result.TryGetProperty("admin1", out var admin1) ? admin1.GetString() ?? "" : "",
                Country = result.TryGetProperty("country", out var country) ? country.GetString() ?? "" : "",
                Latitude = result.GetProperty("latitude").GetDouble(),
                Longitude = result.GetProperty("longitude").GetDouble(),
            });
        }

        return items;
    }
}
