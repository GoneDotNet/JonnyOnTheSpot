using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeoSnappy.Models;
using GeoSnappy.Services;
using Shiny.SqliteDocumentDb;

namespace GeoSnappy.ViewModels;

public partial class PhotoListViewModel : ObservableObject
{
    private readonly IDocumentStore _store;
    private readonly TitleGeneratorService _titleService;

    public PhotoListViewModel(IDocumentStore store, TitleGeneratorService titleService)
    {
        _store = store;
        _titleService = titleService;
    }

    public ObservableCollection<Photo> Photos { get; } = [];

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task LoadPhotosAsync()
    {
        try
        {
            var photos = await _store.GetAll<Photo>();
            var sorted = photos.OrderByDescending(p => p.CapturedAt);

            Photos.Clear();
            foreach (var photo in sorted)
                Photos.Add(photo);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            FileResult? result;

#if TARGET_IPHONE_SIMULATOR || DEBUG
            // On simulator, camera hangs — use photo picker instead
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a GeoSnappy!"
                });
            }
            else
#endif
            {
                // Check and request camera permissions
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                    if (cameraStatus != PermissionStatus.Granted)
                    {
                        await Shell.Current.DisplayAlertAsync("Permission Denied", "Camera permission is required to take photos.", "OK");
                        return;
                    }
                }

                result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a GeoSnappy!"
                });
            }

            if (result is null) return;

            // Save to app storage
            var fileName = $"geosnappy_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await using (var sourceStream = await result.OpenReadAsync())
            await using (var destStream = File.OpenWrite(filePath))
            {
                await sourceStream.CopyToAsync(destStream);
            }

            // Get location
            double latitude = 0, longitude = 0;
            try
            {
                var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                    locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (locationStatus == PermissionStatus.Granted)
                {
                    var location = await Geolocation.Default.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10)));

                    if (location is not null)
                    {
                        latitude = location.Latitude;
                        longitude = location.Longitude;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            }

            // Generate AI title (pass photo for image-aware captioning)
            var title = await _titleService.GenerateFunnyTitleAsync(latitude, longitude, filePath);

            // Save to database (store relative filename, not full path)
            var photo = new Photo
            {
                Title = title,
                FilePath = fileName,
                Latitude = latitude,
                Longitude = longitude,
                CapturedAt = DateTime.UtcNow
            };

            await _store.Set(photo);

            // Refresh list
            await LoadPhotosAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Oops!", $"Something went wrong: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(Photo photo)
    {
        var navParams = new Dictionary<string, object> { { "Photo", photo } };
        await Shell.Current.GoToAsync("detail", navParams);
    }
}
