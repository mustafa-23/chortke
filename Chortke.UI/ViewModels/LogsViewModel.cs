using Chortke.Data;
using DevExpress.Mvvm.POCO;
using Moonlight.MVVM;

namespace Chortke.UI.ViewModels;

public class LogsViewModel : ViewModelBase<LogsViewModel>
{
	public virtual string Logs => DBHandler.GetLog();
	public void ClearLog()
	{
		DBHandler.ClearLog();
		POCOViewModelExtensions.RaisePropertyChanged(this, vm => vm.Logs);
	}
}