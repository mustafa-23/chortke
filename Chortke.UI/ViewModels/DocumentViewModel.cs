using Chortke.Common;
using Chortke.Data;
using Chortke.Models;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Moonlight;
using Moonlight.MVVM;
using System;
using System.Linq;
using System.Windows;

namespace Chortke.UI.ViewModels
{
	public class DocumentViewModel : ViewModelBase<DocumentViewModel>, ISupportParameter
	{
		#region Bindabls
		public virtual DocumentModel Document { get; set; } = default!;
		public virtual object Parameter { get; set; } = default!;
		#endregion

		#region Constructors
		public DocumentViewModel()
		{
			Document = new DocumentModel { CreateDate = DateTime.Now };
		}
		#endregion

		#region Commands
		public void Cancel() => MyModuleManager.ClearTopMost();
		public void Save()
		{
			try
			{
				var mapped = Document.Map<Document>((m, e) => e.Articles = [.. (m as DocumentModel).Articles.Map<ArticleModel, Article>()]);
				DBHandler.SaveDocument(mapped);
				BreakArticleConnection();
				if (MyModuleManager.DefaultManager.IsInjected(Regions.Main, Modules.Documents))
					Messenger.Default.Send(DocumentsViewModel.Action.Update);
				MyModuleManager.ClearTopMost();
			}
			catch (Exception ex)
			{
				var inner = ex.InnerException;
				string message = ex.Message;
				while (inner != null)
				{
					message += inner.Message;
					inner = inner.InnerException;
				}
				MessageBox.Show(message, "خطا در ثبت سند", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public bool CanSave() => Document != null && Document.Debit == Document.Credit;
		public void AddArticle()
		{
			var article = new ArticleModel { CreateDate = Document.CreateDate };
			article.PropertyChanged += Article_PropertyChanged;
			article.Document = Document;
			Document.Articles.Add(article);
		}
		#endregion

		#region Events
		private void Article_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ArticleModel.Book) && sender is ArticleModel article)
				article.IsDebit = !article.Book.IsCredit;
			else if (e.PropertyName != nameof(ArticleModel.CreateDate))
				Document.RaiseDebitCreditChanged();
		}
		#endregion

		#region Methods
		private void BreakArticleConnection() => Document.Articles.ForEach(x => x.PropertyChanged -= Article_PropertyChanged);
		#endregion

		#region Property Change Methods
		protected virtual void OnParameterChanged(object oldValue)
		{
			if (Parameter is DocumentModel doc)
			{
				doc.Articles.ForEach(x =>
				{
					x.PropertyChanged += Article_PropertyChanged;
					x.Book = BookRepository.Instance.GetBook(x.BookId);
					x.Document = doc;
				});
				Document = doc;
			}
			if (oldValue is DocumentModel oldDoc)
				oldDoc.Articles.ForEach(x => x.PropertyChanged -= Article_PropertyChanged);
		}
		#endregion
	}
}
