using GeoSnappy.Services;
using GeoSnappy.ViewModels;
using GeoSnappy.Views;
using Microsoft.Extensions.Logging;
using Shiny.SqliteDocumentDb;

#if DEBUG
using MauiDevFlow.Agent;
#endif

namespace GeoSnappy;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Document store
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "geosnappy.db");
		builder.Services.AddSqliteDocumentStore(opts =>
		{
			opts.ConnectionString = $"Data Source={dbPath}";
			opts.JsonSerializerOptions = AppJsonContext.Default.Options;
		});

		// AI chat client (iOS only — Apple Intelligence)
#if IOS
		builder.Services.AddSingleton<Microsoft.Extensions.AI.IChatClient>(
			sp => new Microsoft.Maui.Essentials.AI.AppleIntelligenceChatClient(
				sp.GetService<ILoggerFactory>()));
#endif

		// Services
		builder.Services.AddSingleton<TitleGeneratorService>();

		// ViewModels
		builder.Services.AddTransient<PhotoListViewModel>();
		builder.Services.AddTransient<PhotoDetailViewModel>();

		// Pages
		builder.Services.AddTransient<PhotoListPage>();
		builder.Services.AddTransient<PhotoDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.AddMauiDevFlowAgent();
#endif

		return builder.Build();
	}
}
