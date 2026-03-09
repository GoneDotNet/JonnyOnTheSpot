using Microsoft.Extensions.Logging;
using MauiDevFlow.Agent;
using StupidHabitTracker.Services;
using StupidHabitTracker.ViewModels;
using StupidHabitTracker.Views;

namespace StupidHabitTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
		builder.Services.AddSingleton<ITimerService, TimerService>();

		// ViewModels
		builder.Services.AddTransient<TimerViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<StatsViewModel>();

		// Pages
		builder.Services.AddTransient<TimerPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<StatsPage>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.AddMauiDevFlowAgent();
#endif

		return builder.Build();
	}
}
