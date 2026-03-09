using StupidHabitTracker.ViewModels;

namespace StupidHabitTracker.Views;

public partial class TimerPage : ContentPage
{
    private readonly TimerViewModel _viewModel;

    public TimerPage(TimerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRecentHabitsCommand.ExecuteAsync(null);
    }
}
