namespace WeatherApp.Models;

public class SavedLocation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsCurrentLocation { get; set; }
    public string DisplayName => string.IsNullOrEmpty(Region) ? $"{Name}, {Country}" : $"{Name}, {Region}";
}
