using Chortke.UI.ViewModels;
using DevExpress.Mvvm.POCO;
using System.Windows.Controls;

namespace Chortke.UI.Views
{
	public partial class DocumentView : UserControl
	{
		public DocumentView()
		{
			InitializeComponent();
			PreviewKeyDown += DocumentView_PreviewKeyDown;
		}

		private void DocumentView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Insert)
			{
				(DataContext as DocumentViewModel).GetCommand(x => x.AddArticle()).Execute(null);
				e.Handled = true;
			}
			else if (e.Key == System.Windows.Input.Key.Escape)
			{
				(DataContext as DocumentViewModel).GetCommand(x => x.Cancel()).Execute(null);
				e.Handled = true;
			}
		}
	}
}
