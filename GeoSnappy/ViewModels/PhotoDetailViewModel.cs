using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeoSnappy.Models;
using Shiny.SqliteDocumentDb;

namespace GeoSnappy.ViewModels;

[QueryProperty(nameof(Photo), "Photo")]
public partial class PhotoDetailViewModel : ObservableObject
{
    private readonly IDocumentStore _store;

    public PhotoDetailViewModel(IDocumentStore store)
    {
        _store = store;
    }

    [ObservableProperty]
    private Photo? _photo;

    [RelayCommand]
    private async Task DeletePhotoAsync()
    {
        if (Photo is null) return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Photo",
            "Are you sure you want to delete this snap?",
            "Delete", "Cancel");

        if (!confirm) return;

        if (File.Exists(Photo.FilePath))
            File.Delete(Photo.FilePath);

        await _store.Remove<Photo>(Photo.Id);
        await Shell.Current.GoToAsync("..");
    }
}
