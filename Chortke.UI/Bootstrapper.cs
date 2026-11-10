using Chortke.Common;
using Chortke.UI.ViewModels;
using Moonlight;
using Moonlight.MVVM;
using System.Threading.Tasks;

namespace Chortke.UI;

public class Bootstrapper : BootstrapperBase
{
	protected override void InjectModules()
	{
		MyModuleManager.DefaultManager.Inject(Regions.Main, Modules.Home);
		MyModuleManager.DefaultManager.Inject(Regions.Header, Modules.ControlBox);
		MyModuleManager.DefaultManager.Inject(Regions.Header, Modules.ModuleSelect);
		MyModuleManager.DefaultManager.Inject(Regions.Footer, Modules.Footer);
	}
	protected override void RegisterModules()
	{
		MyModuleManager.DefaultManager.Register(Regions.Main, BooksViewModel.GetModule(Modules.Books, typeof(Views.BooksView)));
		MyModuleManager.DefaultManager.Register(Regions.Main, DocumentsViewModel.GetModule(Modules.Documents, typeof(Views.DocumentsView)));
		MyModuleManager.DefaultManager.Register(Regions.Main, ArticlesViewModel.GetModule(Modules.Articles, typeof(Views.ArticlesView)));
		MyModuleManager.DefaultManager.Register(Regions.Main, HomeViewModel.GetModule(Modules.Home, typeof(Views.HomeView)));
		MyModuleManager.DefaultManager.Register(Regions.Main, BookRemainReportViewModel.GetModule(Modules.BookRemainReport, typeof(Views.BookRemainReportView)));

		MyModuleManager.DefaultManager.Register(Regions.Topmost, DocumentViewModel.GetModule(Modules.Document, typeof(Views.DocumentView)));
		MyModuleManager.DefaultManager.Register(Regions.Topmost, YesNoViewModel.GetModule(Modules.YesNo, typeof(Views.YesNoView)));

		MyModuleManager.DefaultManager.Register(Regions.Header, ControlBoxViewModel.GetModule(Modules.ControlBox, typeof(Views.ControlBoxView)));
		MyModuleManager.DefaultManager.Register(Regions.Footer, FooterViewModel.GetModule(Modules.Footer, typeof(Views.FooterView)));
		MyModuleManager.DefaultManager.Register(Regions.Header, ModuleSelectViewModel.GetModule(Modules.ModuleSelect, typeof(Views.ModuleSelectView)));
	}
	protected override async Task ShowMainWindowAsync()
	{
		await BookRepository.Instance.InitializeAsync();
		UIManager.MainWindow.Show();
	}
	protected override void CreateShell() => UIManager.CurrentApp.MainWindow = new Shell();
}