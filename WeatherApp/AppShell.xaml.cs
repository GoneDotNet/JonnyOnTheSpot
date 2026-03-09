using WeatherApp.Views;

namespace WeatherApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(WeatherDetailPage), typeof(WeatherDetailPage));
		Routing.RegisterRoute(nameof(AddLocationPage), typeof(AddLocationPage));
		Routing.RegisterRoute(nameof(RadarMapPage), typeof(RadarMapPage));
	}
}
