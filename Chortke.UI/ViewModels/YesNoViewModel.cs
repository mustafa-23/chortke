using Chortke.Common;
using Chortke.Models;
using DevExpress.Mvvm;
using Moonlight.MVVM;

namespace Chortke.UI.ViewModels
{
	public class YesNoViewModel : ViewModelBase<YesNoViewModel>, ISupportParameter
	{
		public virtual object Parameter { get; set; } = default!;
		public virtual MessageModel Message { get; set; } = default!;
		protected virtual void OnParameterChanged(object old)
		{
			Message = Parameter as MessageModel ?? default!;
		}

		public void Close(bool result)
		{
			Message.Listener?.OnDialogClosed(result);
			MyModuleManager.ClearTopMost();
		}
	}

	public interface IYesNoDialogListener
	{
		void OnDialogClosed(bool result);
	}
}
