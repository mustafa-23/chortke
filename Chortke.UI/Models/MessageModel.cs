using Chortke.UI.ViewModels;

namespace Chortke.Models
{
	public class MessageModel(string title, string message, IYesNoDialogListener? listener)
	{
		public string Title { get; set; } = title;
		public string Message { get; set; } = message;
		public IYesNoDialogListener Listener { get; } = listener;
	}
}
