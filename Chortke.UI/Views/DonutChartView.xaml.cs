using Chortke.Data;
using Chortke.Models;
using Chortke.UI.Common;
using Moonlight;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chortke.UI.Views
{
	public partial class DonutChartView : UserControl, IStorable, INotifyPropertyChanged
	{
		private Point lastPose = new(0, 0);
		private static readonly string[] COLORS = ["#ff7d81", "#8acc7d", "#c6c63f", "#5a67e1", "#da58e6",
			"#800000"];

		public event PropertyChangedEventHandler? PropertyChanged;

		public ObservableCollection<BookModel> Books { get; } = [];
		public BookModel Selected { get; set; } = default!;
		private string _title = "Untitled";
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
			}
		}

		#region Ctors
		public DonutChartView()
		{
			InitializeComponent();
			Loaded += (s, e) => Update();
			Books.CollectionChanged += Books_CollectionChanged;
		}
		~DonutChartView()
		{
			Books.CollectionChanged -= Books_CollectionChanged;
		}
		#endregion

		private void Update()
		{
			LabelStyle.RTLSupport = true;
			List<PieSlice> slices = [];
			long total = 0;
			int index = 0;
			foreach (var book in Books)
			{
				var articles = DBHandler.GetArticlesByBookId(book.Id);
				var sum = Math.Abs(articles.Sum(x => (x.IsDebit ? -1 : 1) * x.Amount) + ((book.IsCredit ? 1 : -1) * book.InitialAmount));
				slices.Add(new PieSlice(sum, Color.FromHex(COLORS[index]), ""));
				total += sum;
				index = (index + 1) % COLORS.Length;
			}
			foreach (var slice in slices)
				slice.Label = $"{(slice.Value / total):%0.0}\n{slice.Value:N0}";
			plot.Plot.Clear();
			var pie = plot.Plot.Add.Pie(slices);
			pie.DonutFraction = 0.5;
			pie.ExplodeFraction = 0.04;
			pie.LineStyle = LineStyle.None;
			pie.SliceLabelDistance = 0.75;

			plot.Plot.Axes.Frameless();
			plot.Plot.HideGrid();
			plot.Plot.FigureBackground = new BackgroundStyle { Color = Colors.Transparent };
			plot.Plot.Legend.BackgroundColor = Colors.Transparent;
			plot.Plot.Legend.ShadowColor = Colors.Transparent;
			plot.Plot.Legend.OutlineWidth = 0;
			plot.Plot.ScaleFactor = 1;
			plot.Plot.Axes.AutoScale();
			plot.Refresh();
		}

		#region Events
		private void ResizeBorder_MouseMove(object sender, MouseEventArgs e)
		{
			var p1 = e.GetPosition(ResizeBorder);
			var px = p1.X / ResizeBorder.ActualWidth;
			var py = p1.Y / ResizeBorder.ActualHeight;
			var resizeDir = -1;
			ResizeBorder.Cursor = Cursors.Arrow;
			if (px < 0.1 && py > 0.98)
				resizeDir = 2;
			else if (px < 0.1)
				resizeDir = 0;
			else if (py > 0.98)
				resizeDir = 1;
			if (resizeDir != -1)
				ResizeBorder.Cursor = resizeDir == 2 ? Cursors.SizeNWSE : (resizeDir == 0 ? Cursors.SizeWE : Cursors.SizeNS);

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				var current = e.GetPosition(Application.Current.MainWindow);
				var dx = lastPose.X - current.X;
				var dy = current.Y - lastPose.Y;
				if (resizeDir == 0 || resizeDir == 2)
					Width += dx;
				if (resizeDir == 1 || resizeDir == 2)
					Height += dy;
				lastPose = current;
			}
			else
				lastPose = e.GetPosition(Application.Current.MainWindow);
		}
		private void bookSelect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Books.Add(bookSelect.SelectedBook);
		}
		private void Books_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Update();
		}

		#endregion

		#region IStorable
		public string Save()
		{
			var model = new ChartSaveModel { Title = Title, BookIds = string.Join(',', Books.Select(x => x.Id)) };
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(model);
			return json;
		}
		public void Restore(string json)
		{
			var model = Newtonsoft.Json.JsonConvert.DeserializeObject<ChartSaveModel>(json);
			if (model != null)
			{
				Title = model.Title;
				foreach (var id in model.BookIds.Split(',').Select(int.Parse))
				{
					var book = DBHandler.GetBook(id);
					if (book != null)
						Books.Add(book.Map<BookModel>());
				}
			}
		}
		#endregion

		private class ChartSaveModel
		{
			public string Title { get; set; } = default!;
			public string BookIds { get; set; } = default!;
		}
	}
}