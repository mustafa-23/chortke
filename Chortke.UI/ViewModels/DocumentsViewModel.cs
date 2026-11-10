using Chortke.Common;
using Chortke.Data;
using Chortke.Models;
using Chortke.UI.Common;
using DevExpress.Mvvm;
using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Mvvm.Native;
using Moonlight;
using Moonlight.MVVM;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Chortke.UI.ViewModels
{
	public class DocumentsViewModel : ViewModel<DocumentsViewModel>, IYesNoDialogListener
	{
		public enum Action { Update }
		public ObservableCollection<DocumentModel> Documents { get; } = new();
		public virtual DocumentModel SelectedDocument { get; set; } = default!;
		public ICollectionView DocumentsListViewSource { get; set; }
		public DocumentsViewModel()
		{
			FillDocumentsAsync();
			SetViewSource();
			MyModuleManager.DefaultManager.GetEvents(this).ViewModelRemoving += DocumentsViewModel_ViewModelRemoving;
			Messenger.Default.Register<Action>(this, null, MessageReceived);
		}

		private void SetViewSource()
		{
			DocumentsListViewSource = CollectionViewSource.GetDefaultView(Documents);
			DocumentsListViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DocumentModel.CreateDate), new DateToPartConverter { PartName = "year" }));
			DocumentsListViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DocumentModel.CreateDate), new DateToPartConverter()));
		}

		private void MessageReceived(Action action) => FillDocumentsAsync();
		private void DocumentsViewModel_ViewModelRemoving(object? sender, ViewModelRemovingEventArgs e) => Messenger.Default.Unregister(this);
		private async Task FillDocumentsAsync()
		{
			IsLoading = true;
			try
			{
				var docs = (await Task.Run(DBHandler.GetDocuments)).ToList();

				var mapped = docs.Map<Document, DocumentModel>((s, d) => d.Articles.AddRange((s as Document).Articles.Map<Article, ArticleModel>())).ToList();
				mapped.ForEach(x => x.Articles.ForEach(a => a.Book = BookRepository.Instance.GetBook(a.BookId)));
				Documents.Reset(mapped.OrderBy(x => x.CreateDate));
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

		#region Commands
		public void DeleteDocument()
		{
			var title = string.Format(UIManager.GetResource<string>("DocumentDeleteMessage"), SelectedDocument.Id);
			MyModuleManager.ShowYesNoDialog(title, UIManager.GetResource<string>("UndoneMessage"), this);
		}
		public bool CanDeleteDocument() => SelectedDocument != null;

		public void OnDialogClosed(bool result)
		{
			if (result)
				SelectedDocument.DeleteCommand.Execute(null);
		}
		#endregion
	}
}
