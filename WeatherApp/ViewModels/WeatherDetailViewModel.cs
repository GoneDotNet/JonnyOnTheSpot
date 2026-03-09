using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Models;
using WeatherApp.Services;
using WeatherApp.Views;

namespace WeatherApp.ViewModels;

[QueryProperty(nameof(Location), "Location")]
public partial class WeatherDetailViewModel : ObservableObject
{
    private readonly IWeatherService _weatherService;

    [ObservableProperty]
    private SavedLocation? _location;

    [ObservableProperty]
    private WeatherData? _weather;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _backgroundGradientStart = "#4A90D9";

    [ObservableProperty]
    private string _backgroundGradientEnd = "#1E3A5F";

    public WeatherDetailViewModel(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    partial void OnLocationChanged(SavedLocation? value)
    {
        if (value != null)
            _ = LoadWeatherAsync();
    }

    [RelayCommand]
    private async Task LoadWeatherAsync()
    {
        if (Location == null) return;

        try
        {
            IsLoading = true;
            Weather = await _weatherService.GetWeatherAsync(Location.Latitude, Location.Longitude);
            UpdateBackgroundGradient();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading weather: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateBackgroundGradient()
    {
        if (Weather?.Current == null) return;

        var code = Weather.Current.WeatherCode;
        var hour = DateTime.Now.Hour;
        var isNight = hour < 6 || hour > 20;

        (BackgroundGradientStart, BackgroundGradientEnd) = (code, isNight) switch
        {
            (0, true) => ("#0F2027", "#203A43"),          // Clear night
            (0, false) => ("#56CCF2", "#2F80ED"),          // Clear day
            (1 or 2, true) => ("#1A1A2E", "#16213E"),     // Partly cloudy night
            (1 or 2, false) => ("#89ABE3", "#5B86C5"),    // Partly cloudy day
            (3, _) => ("#606C88", "#3F4C6B"),              // Overcast
            (45 or 48, _) => ("#757F9A", "#D7DDE8"),       // Fog
            (>= 51 and <= 67, _) => ("#3A6186", "#89253E"),// Rain
            (>= 71 and <= 77, _) => ("#E6DADA", "#274046"),// Snow
            (>= 80 and <= 82, _) => ("#373B44", "#4286F4"),// Showers
            (>= 95, _) => ("#232526", "#414345"),           // Thunderstorm
            _ => ("#4A90D9", "#1E3A5F"),
        };
    }

    [RelayCommand]
    private async Task OpenRadarMapAsync()
    {
        if (Location == null) return;
        var navParams = new Dictionary<string, object>
        {
            { "Location", Location }
        };
        await Shell.Current.GoToAsync(nameof(RadarMapPage), navParams);
    }
}
