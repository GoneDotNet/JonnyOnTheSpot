using GeoSnappy.Views;

namespace GeoSnappy;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("detail", typeof(PhotoDetailPage));
	}
}
