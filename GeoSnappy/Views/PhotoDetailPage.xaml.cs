using GeoSnappy.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace GeoSnappy.Views;

public partial class PhotoDetailPage : ContentPage
{
    public PhotoDetailPage(PhotoDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PhotoDetailViewModel vm && vm.Photo is not null)
        {
            var position = new Location(vm.Photo.Latitude, vm.Photo.Longitude);
            PhotoMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(1)));
            PhotoMap.Pins.Add(new Pin
            {
                Label = vm.Photo.Title,
                Location = position,
                Type = PinType.Place
            });
        }
    }
}
