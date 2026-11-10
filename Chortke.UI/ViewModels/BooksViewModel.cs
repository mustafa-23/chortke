using Chortke.Common;
using Chortke.Data;
using Chortke.Models;
using Chortke.UI.Common;
using Chortke.UserControls;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using Moonlight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Chortke.UI.ViewModels
{
	public class BooksViewModel : ViewModel<BooksViewModel>
	{
		private static int Count = 0;
		public int Id { get; set; }
		public virtual BookModel CurrentBook { get; set; } = default!;
		public virtual decimal BookRemain { get; set; }
		public virtual decimal SumDebit { get; set; }
		public virtual decimal SumCredit { get; set; }
		public virtual long InitialAmount { get; set; }
		private ObservableCollection<ArticleModel> _articles { get; } = [];
		public ICollectionView Articles { get; }

		public BooksViewModel()
		{
			Articles = CollectionViewSource.GetDefaultView(_articles);
			PrepareArticlesView();
			Application.Current.Exit += Current_Exit;
			MyModuleManager.DefaultManager.GetEvents(this).ViewModelRemoved += BooksViewModel_ViewModelRemoved;
			Id = Count++;
		}

		private void PrepareArticlesView()
		{
			Articles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DocumentModel.CreateDate), new DateToPartConverter { PartName = "year" }));
			Articles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Article.CreateDate), new DateToPartConverter()));
		}

		#region Events
		private void Current_Exit(object sender, ExitEventArgs e) => SaveChanges();
		private void BooksViewModel_ViewModelRemoved(object? sender, DevExpress.Mvvm.ModuleInjection.ViewModelRemovedEventArgs e)
		{
			SaveChanges();
			MyModuleManager.DefaultManager.GetEvents(this).ViewModelRemoved -= BooksViewModel_ViewModelRemoved;
			Application.Current.Exit -= Current_Exit;
		}
		#endregion

		#region Commands
		public void AddBook() => Messenger.Default.Send(new BookSelectControlMessage { Action = BookSelectControl.Action.Add, Tag = "BooksView" });
		public void SaveChanges() => Messenger.Default.Send(new BookSelectControlMessage { Action = BookSelectControl.Action.Save, Tag = "BooksView" });
		public void Delete() => Messenger.Default.Send(new BookSelectControlMessage { Action = BookSelectControl.Action.Delete, Tag = "BooksView" });
		public bool CanDelete() => CurrentBook != null;
		#endregion

		protected virtual void OnCurrentBookChanged(BookModel old)
		{
			BookRemain = SumCredit = SumDebit = 0;
			if (CurrentBook?.Id > 0)
			{
				var childrenIds = GetChildrenIds(CurrentBook);
				var tempList = new List<ArticleModel>();
				foreach (var item in childrenIds)
					tempList.AddRange(DBHandler.GetArticlesByBookId(item).Map<Article, ArticleModel>());
				_articles.Reset(tempList.OrderBy(x => x.CreateDate));
				tempList.ForEach(x =>
				{
					if (x.IsCredit)
						SumCredit += x.Amount;
					else
						SumDebit += x.Amount;
				});
				InitialAmount = BookRepository.Instance.GetBooksByIds(childrenIds).Sum(x => x.InitialAmount);
				BookRemain = Math.Abs(((CurrentBook.IsCredit ? 1 : -1) * InitialAmount) + SumCredit - SumDebit);
			}
		}

		private List<int> GetChildrenIds(BookModel book)
		{
			var childrenIds = new List<int>();
			if (book.Children?.Count > 0)
			{
				foreach (var child in book.Children)
					childrenIds.AddRange(GetChildrenIds(child));
				return childrenIds;
			}
			else
				return [book.Id];
		}
	}
}
