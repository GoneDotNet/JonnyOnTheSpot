using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class WeatherDetailPage : ContentPage
{
    public WeatherDetailPage(WeatherDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
