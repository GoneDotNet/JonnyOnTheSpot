using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels;

public partial class AddLocationViewModel : ObservableObject
{
    private readonly IGeocodingService _geocodingService;
    private readonly ILocationStorageService _locationStorage;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    public ObservableCollection<GeocodingResult> SearchResults { get; } = [];

    public AddLocationViewModel(IGeocodingService geocodingService, ILocationStorageService locationStorage)
    {
        _geocodingService = geocodingService;
        _locationStorage = locationStorage;
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = SearchWithDebounceAsync(value);
    }

    private async Task SearchWithDebounceAsync(string query)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;

            IsSearching = true;
            var results = await _geocodingService.SearchAsync(query);

            if (token.IsCancellationRequested) return;

            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectLocationAsync(GeocodingResult result)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            var location = new SavedLocation
            {
                Name = result.Name,
                Region = result.Admin1,
                Country = result.Country,
                Latitude = result.Latitude,
                Longitude = result.Longitude,
            };

            await _locationStorage.SaveLocationAsync(location);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private bool _isNavigating;

    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted) return;

            var geoLocation = await Geolocation.GetLastKnownLocationAsync()
                ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

            if (geoLocation == null) return;

            var location = new SavedLocation
            {
                Name = "Current Location",
                Latitude = geoLocation.Latitude,
                Longitude = geoLocation.Longitude,
                IsCurrentLocation = true,
            };

            await _locationStorage.SaveLocationAsync(location);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting current location: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
