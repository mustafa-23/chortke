using Chortke.UI.ViewModels;
using DevExpress.Mvvm;
using DevExpress.Mvvm.ModuleInjection;

namespace Chortke.Common
{
	public class MyModuleManager : BindableBase
	{
		private const string TOPMOST_VISIBILE = "Visible";
		private const string TOPMOST_HIDDEN = "Hidden";

		private static MyModuleManager _instance = default!;
		public static MyModuleManager Instance => _instance ??= new MyModuleManager();
		public static IModuleManager DefaultManager => ModuleManager.DefaultManager;
		public string TopmostState { get => GetProperty(() => TopmostState); set => SetProperty(() => TopmostState, value); }

		private MyModuleManager()
		{
			TopmostState = TOPMOST_HIDDEN;
		}

		public static void InjectMain(string moduleKey)
		{
			//DefaultManager.Clear(Regions.Main);
			DefaultManager.InjectOrNavigate(Regions.Main, moduleKey);
		}
		public static void InjectTopMost(string moduleKey, object parameter = default!)
		{
			Instance.TopmostState = TOPMOST_VISIBILE;
			DefaultManager.Clear(Regions.Topmost);
			DefaultManager.InjectOrNavigate(Regions.Topmost, moduleKey, parameter);
		}
		public static void ClearTopMost()
		{
			Instance.TopmostState = TOPMOST_HIDDEN;
			DefaultManager.Clear(Regions.Topmost);
		}
		public static void ShowYesNoDialog(string title, string message, IYesNoDialogListener listener)
		{
			InjectTopMost(Modules.YesNo, new Models.MessageModel(title, message, listener));
		}
	}
}
