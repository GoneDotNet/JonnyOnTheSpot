using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Models;
using WeatherApp.Services;
using WeatherApp.Views;

namespace WeatherApp.ViewModels;

public partial class LocationsViewModel : ObservableObject
{
    private readonly IWeatherService _weatherService;
    private readonly ILocationStorageService _locationStorage;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private LocationWeatherSummary? _currentLocationSummary;

    [ObservableProperty]
    private bool _hasCurrentLocation;

    public ObservableCollection<LocationWeatherSummary> Locations { get; } = [];

    public LocationsViewModel(IWeatherService weatherService, ILocationStorageService locationStorage)
    {
        _weatherService = weatherService;
        _locationStorage = locationStorage;
    }

    [RelayCommand]
    private async Task LoadLocationsAsync()
    {
        if (!await _loadLock.WaitAsync(0)) return;

        try
        {
            IsRefreshing = true;

            await LoadCurrentLocationAsync();

            var savedLocations = await _locationStorage.GetLocationsAsync();
            Locations.Clear();

            var tasks = savedLocations.Select(loc => _weatherService.GetWeatherSummaryAsync(loc));
            var summaries = await Task.WhenAll(tasks);

            foreach (var summary in summaries)
            {
                Locations.Add(summary);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading locations: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
            _loadLock.Release();
        }
    }

    private async Task LoadCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                var location = await Geolocation.GetLastKnownLocationAsync()
                    ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

                if (location != null)
                {
                    var savedLoc = new SavedLocation
                    {
                        Name = "Current Location",
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        IsCurrentLocation = true,
                    };

                    CurrentLocationSummary = await _weatherService.GetWeatherSummaryAsync(savedLoc);
                    HasCurrentLocation = true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting current location: {ex.Message}");
            HasCurrentLocation = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(LocationWeatherSummary summary)
    {
        var navParams = new Dictionary<string, object>
        {
            { "Location", summary.Location }
        };
        await Shell.Current.GoToAsync(nameof(WeatherDetailPage), navParams);
    }

    [RelayCommand]
    private async Task NavigateToCurrentDetailAsync()
    {
        if (CurrentLocationSummary?.Location == null) return;
        var navParams = new Dictionary<string, object>
        {
            { "Location", CurrentLocationSummary.Location }
        };
        await Shell.Current.GoToAsync(nameof(WeatherDetailPage), navParams);
    }

    [RelayCommand]
    private async Task RemoveLocationAsync(LocationWeatherSummary summary)
    {
        await _locationStorage.RemoveLocationAsync(summary.Location.Id);
        Locations.Remove(summary);
    }

    [RelayCommand]
    private async Task AddLocationAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddLocationPage));
    }
}
