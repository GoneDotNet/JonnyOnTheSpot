namespace WeatherApp.Models;

public class GeocodingResult
{
    public string Name { get; set; } = string.Empty;
    public string Admin1 { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DisplayName => string.IsNullOrEmpty(Admin1)
        ? $"{Name}, {Country}"
        : $"{Name}, {Admin1}, {Country}";
}
