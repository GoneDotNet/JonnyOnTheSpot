using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class AddLocationPage : ContentPage
{
    public AddLocationPage(AddLocationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
