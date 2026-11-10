using Chortke.UI.Common;
using DevExpress.Mvvm.Native;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Chortke.UI.Views
{
	public partial class HomeView : UserControl
	{
		private const string CHART_PATH = "charts.txt";
		public HomeView()
		{
			InitializeComponent();
			Loaded += HomeView_Loaded;
			Application.Current.Exit += Current_Exit;
		}

		#region Events
		private void HomeView_Loaded(object sender, RoutedEventArgs e)
		{
			if (File.Exists(CHART_PATH))
			{
				foreach (var json in File.ReadAllText(CHART_PATH).Split('\n', System.StringSplitOptions.RemoveEmptyEntries))
				{
					var chart = new DonutChartView();
					chart.Restore(json);
					mainPanel.Children.Add(chart);
				}
			}
		}
		private void Current_Exit(object sender, ExitEventArgs e)
		{
			var text = string.Join('\n', mainPanel.Children.OfType<IStorable>().Select(x => x.Save()));
			File.WriteAllText(CHART_PATH, text, System.Text.Encoding.UTF8);
		}
		private void btnAdd_Click(object sender, RoutedEventArgs e) => mainPanel.Children.Add(new DonutChartView());
		#endregion
	}
}
