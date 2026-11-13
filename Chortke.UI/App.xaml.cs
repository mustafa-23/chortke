using DevExpress.Xpf.Core;
using Moonlight;
using System.IO;
using System.Windows;

namespace Chortke.UI;

public partial class App : Application
{
	protected override async void OnStartup(StartupEventArgs e)
	{
		try
		{
			base.OnStartup(e);
			Dispatcher.UnhandledException += Dispatcher_UnhandledException;
			UIManager.Default.SetLocalizationPath("Localizations");
			UIManager.Default.Initialize();
			UIManager.Default.SelectedLanguage = "fa-IR";
			ApplicationThemeHelper.ApplicationThemeName = Theme.NoneName;
			await new Bootstrapper().Run();
		}
		catch (System.Exception ex)
		{
			var msg = $"Error Message:\n{ex.Message}\nStackTrace:{ex.StackTrace}";
			MessageBox.Show(msg,"StartUp Unhandled Error");
			Current.Shutdown();
		}
	}

	private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		var msg = $"Error Message:\n{e.Exception.Message}\nStackTrace:{e.Exception.StackTrace}";
		MessageBox.Show(msg, "Dispatcher Unhandled Error");
		Current.Shutdown();
	}
}