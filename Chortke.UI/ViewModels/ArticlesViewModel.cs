using Chortke.Data;
using Chortke.Models;
using Chortke.UI.Common;
using Moonlight;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Chortke.UI.ViewModels
{
	public class ArticlesViewModel : ViewModel<ArticlesViewModel>
	{
		public ObservableCollection<ArticleModel> Articles { get; } = [];
		public virtual long TotalDebit { get; set; }
		public virtual long TotalCredit { get; set; }
		public Filter Filter { get; } = new();
		public ArticlesViewModel()
		{
			var now = DateTime.Now;
			Filter.FromDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
			Filter.ToDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
			FillArticlesAsync();
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
				var docs = (await Task.Run(() => DBHandler.GetArticles(Filter))).ToList();
				var mapped = docs.Map<Article, ArticleModel>().ToList();
				Articles.Reset(mapped.OrderBy(x => x.CreateDate));
				TotalCredit = Articles.Where(x => x.IsCredit).Sum(x => x.Amount);
				TotalDebit = Articles.Where(x => x.IsDebit).Sum(x => x.Amount);
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
