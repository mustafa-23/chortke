using DevExpress.Xpf.Core;
using Moonlight;
using System.IO;
using System.Windows;

namespace Chortke.UI;

public partial class App : Application
{
	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		UIManager.Default.SetLocalizationPath("Localizations");
		UIManager.Default.Initialize();
		UIManager.Default.SelectedLanguage = "fa-IR";
		ApplicationThemeHelper.ApplicationThemeName = Theme.NoneName;
		await new Bootstrapper().Run();
	}
}