using Chortke.Common;
using Moonlight.MVVM;
using System.Windows;

namespace Chortke.UI.Common
{
	public class ViewModel<T> : ViewModelBase<T> where T : class, new()
	{
		public virtual Visibility Visibility { get; set; }
		public ViewModel()
		{
			MyModuleManager.DefaultManager.GetEvents(this).Navigated += ViewModel_Navigated;
			MyModuleManager.DefaultManager.GetEvents(this).NavigatedAway += ViewModel_NavigatedAway;
		}
		private void ViewModel_NavigatedAway(object? sender, DevExpress.Mvvm.ModuleInjection.NavigationEventArgs e)
		{
			Visibility = Visibility.Collapsed;
		}
		private void ViewModel_Navigated(object? sender, DevExpress.Mvvm.ModuleInjection.NavigationEventArgs e)
		{
			Visibility = Visibility.Visible;
		}
		~ViewModel()
		{
			MyModuleManager.DefaultManager.GetEvents(this).Navigated -= ViewModel_Navigated;
			MyModuleManager.DefaultManager.GetEvents(this).NavigatedAway -= ViewModel_NavigatedAway;
		}
	}
}
