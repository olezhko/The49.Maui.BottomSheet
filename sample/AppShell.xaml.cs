namespace The49.Maui.BottomSheet.Sample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
	}
}
