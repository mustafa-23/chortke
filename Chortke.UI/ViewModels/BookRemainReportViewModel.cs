using Chortke.Data;
using Chortke.Models;
using Chortke.UI.Common;
using Moonlight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chortke.UI.ViewModels
{
	public class BookRemainReportViewModel : ViewModel<BookRemainReportViewModel>
	{
		public ObservableCollection<ArticleModel> Articles { get; } = [];
		public Filter Filter { get; } = new();
		public virtual BookModel SelectedBook { get; set; } = default!;
		public virtual double TotalDebit { get; set; }
		public virtual double TotalCredit { get; set; }
		public virtual double Remain { get; set; }

		public BookRemainReportViewModel()
		{
			var now = DateTime.Now;
			Filter.ToDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
		}

		public async Task Search()
		{
			await FillArticlesAsync();
		}
		private async Task FillArticlesAsync()
		{
			IsLoading = true;
			try
			{
				Filter.BookIds = [SelectedBook.Id];
				var docs = (await Task.Run(() => DBHandler.GetArticles(Filter))).ToList();
				var mapped = docs.Map<Article, ArticleModel>().ToList();
				Articles.Reset(mapped.OrderBy(x => x.CreateDate));
				TotalCredit = Articles.Where(x => x.IsCredit).Sum(x => (double)x.Amount);
				TotalDebit = Articles.Where(x => x.IsDebit).Sum(x => (double)x.Amount);
				Remain = SelectedBook.InitialAmount + ((SelectedBook.IsCredit ? 1 : -1) * (TotalCredit - TotalDebit));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				IsLoading = false;
			}
		}
	}
}
