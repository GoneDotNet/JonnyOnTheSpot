using WeatherApp.Models;

namespace WeatherApp.Services;

public interface IRainViewerService
{
    Task<List<RadarFrame>> GetRadarFramesAsync();
}
