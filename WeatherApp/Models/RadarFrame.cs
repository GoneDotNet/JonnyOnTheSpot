namespace WeatherApp.Models;

public class RadarFrame
{
    public string Host { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Time { get; set; }

    public string GetTileUrl(int size = 512, int z = 0, int x = 0, int y = 0, int color = 6, int smooth = 1, int snow = 1)
        => $"{Host}{Path}/{size}/{z}/{x}/{y}/{color}/{smooth}_{snow}.png";

    public string GetTileUrlTemplate(int size = 512, int color = 6, int smooth = 1, int snow = 1)
        => $"{Host}{Path}/{size}/{{z}}/{{x}}/{{y}}/{color}/{smooth}_{snow}.png";
}
