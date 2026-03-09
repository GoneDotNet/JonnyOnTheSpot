using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class LocationStorageService : ILocationStorageService
{
    private const string StorageKey = "saved_locations";

    public Task<List<SavedLocation>> GetLocationsAsync()
    {
        var json = Preferences.Default.Get(StorageKey, "[]");
        var locations = JsonSerializer.Deserialize<List<SavedLocation>>(json) ?? [];
        return Task.FromResult(locations);
    }

    public async Task SaveLocationAsync(SavedLocation location)
    {
        var locations = await GetLocationsAsync();

        // Don't add duplicates (check by name+country or close coordinates)
        if (locations.Any(l => (l.Name == location.Name && l.Country == location.Country)
                            || (Math.Abs(l.Latitude - location.Latitude) < 0.01
                                && Math.Abs(l.Longitude - location.Longitude) < 0.01)))
            return;

        locations.Add(location);
        var json = JsonSerializer.Serialize(locations);
        Preferences.Default.Set(StorageKey, json);
    }

    public async Task RemoveLocationAsync(string locationId)
    {
        var locations = await GetLocationsAsync();
        locations.RemoveAll(l => l.Id == locationId);
        var json = JsonSerializer.Serialize(locations);
        Preferences.Default.Set(StorageKey, json);
    }
}
