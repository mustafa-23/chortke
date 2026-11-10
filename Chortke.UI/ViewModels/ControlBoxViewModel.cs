using DevExpress.Mvvm.POCO;
using Moonlight;
using System.Windows;

namespace Chortke.UI.ViewModels
{
	public class ControlBoxViewModel : Moonlight.MVVM.ViewModelBase<ControlBoxViewModel>
	{
		public ControlBoxViewModel() { }

		#region Commands
		public void Minimize() => UIManager.CurrentApp.MainWindow.WindowState = WindowState.Minimized;
		public void Maximize()
		{
			if (UIManager.MainWindow.WindowState == WindowState.Maximized)
				UIManager.MainWindow.WindowState = WindowState.Normal;
			else
				UIManager.MainWindow.WindowState = WindowState.Maximized;

		}
		public void ChangeLanguage(string lang) => UIManager.Default.SelectedLanguage = lang;
		#endregion
	}
}
