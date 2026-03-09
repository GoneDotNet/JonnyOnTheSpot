using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels;

[QueryProperty(nameof(Location), "Location")]
public partial class RadarMapViewModel : ObservableObject
{
    private readonly IRainViewerService _rainViewerService;

    [ObservableProperty]
    private SavedLocation? _location;

    [ObservableProperty]
    private string _radarFramesJson = "[]";

    [ObservableProperty]
    private bool _isLoading = true;

    public RadarMapViewModel(IRainViewerService rainViewerService)
    {
        _rainViewerService = rainViewerService;
    }

    partial void OnLocationChanged(SavedLocation? value)
    {
        if (value != null)
            _ = LoadRadarDataAsync();
    }

    [RelayCommand]
    private async Task LoadRadarDataAsync()
    {
        try
        {
            IsLoading = true;
            var frames = await _rainViewerService.GetRadarFramesAsync();
            var frameData = frames.Select(f => new
            {
                tileUrl = f.GetTileUrlTemplate(),
                time = f.Time,
            });
            RadarFramesJson = JsonSerializer.Serialize(frameData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading radar data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
