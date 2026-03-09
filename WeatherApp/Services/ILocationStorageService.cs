using WeatherApp.Models;

namespace WeatherApp.Services;

public interface ILocationStorageService
{
    Task<List<SavedLocation>> GetLocationsAsync();
    Task SaveLocationAsync(SavedLocation location);
    Task RemoveLocationAsync(string locationId);
}
