using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using WeatherApp.Services;
using WeatherApp.ViewModels;
using WeatherApp.Views;
#if DEBUG
using MauiDevFlow.Agent;
#endif

namespace WeatherApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// HTTP client
		builder.Services.AddHttpClient("OpenMeteo", client =>
		{
			client.BaseAddress = new Uri("https://api.open-meteo.com");
		});
		builder.Services.AddHttpClient("OpenMeteoAirQuality", client =>
		{
			client.BaseAddress = new Uri("https://air-quality-api.open-meteo.com");
		});
		builder.Services.AddHttpClient("OpenMeteoGeocoding", client =>
		{
			client.BaseAddress = new Uri("https://geocoding-api.open-meteo.com");
		});
		builder.Services.AddHttpClient("RainViewer", client =>
		{
			client.BaseAddress = new Uri("https://api.rainviewer.com");
		});

		// Services
		builder.Services.AddSingleton<IWeatherService, OpenMeteoService>();
		builder.Services.AddSingleton<IGeocodingService, OpenMeteoGeocodingService>();
		builder.Services.AddSingleton<IRainViewerService, RainViewerService>();
		builder.Services.AddSingleton<ILocationStorageService, LocationStorageService>();

		// ViewModels
		builder.Services.AddTransient<LocationsViewModel>();
		builder.Services.AddTransient<WeatherDetailViewModel>();
		builder.Services.AddTransient<AddLocationViewModel>();
		builder.Services.AddTransient<RadarMapViewModel>();

		// Pages
		builder.Services.AddTransient<LocationsPage>();
		builder.Services.AddTransient<WeatherDetailPage>();
		builder.Services.AddTransient<AddLocationPage>();
		builder.Services.AddTransient<RadarMapPage>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.AddMauiDevFlowAgent();
#endif

		return builder.Build();
	}
}
