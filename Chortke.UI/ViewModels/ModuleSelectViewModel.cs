using Chortke.Common;
using Moonlight.MVVM;

namespace Chortke.UI.ViewModels
{
	public class ModuleSelectViewModel : ViewModelBase<ModuleSelectViewModel>
	{
		public ModuleSelectViewModel() { }
		public void ChangeModule(string moduleKey) => MyModuleManager.InjectMain(moduleKey);
	}
}
