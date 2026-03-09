using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class RainViewerService : IRainViewerService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RainViewerService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<RadarFrame>> GetRadarFramesAsync()
    {
        var client = _httpClientFactory.CreateClient("RainViewer");
        var json = await client.GetStringAsync("/public/weather-maps.json");
        var doc = JsonDocument.Parse(json);

        var host = doc.RootElement.GetProperty("host").GetString() ?? "";
        var frames = new List<RadarFrame>();

        if (doc.RootElement.TryGetProperty("radar", out var radar) &&
            radar.TryGetProperty("past", out var past))
        {
            foreach (var frame in past.EnumerateArray())
            {
                frames.Add(new RadarFrame
                {
                    Host = host,
                    Path = frame.GetProperty("path").GetString() ?? "",
                    Time = frame.GetProperty("time").GetInt64(),
                });
            }
        }

        return frames;
    }
}
