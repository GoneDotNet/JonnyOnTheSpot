using GeoSnappy.ViewModels;

namespace GeoSnappy.Views;

public partial class PhotoListPage : ContentPage
{
    public PhotoListPage(PhotoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PhotoListViewModel vm)
            vm.LoadPhotosCommand.Execute(null);
    }
}
