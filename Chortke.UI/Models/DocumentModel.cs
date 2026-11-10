using Chortke.Common;
using Chortke.Data;
using Chortke.UI.Common;
using Chortke.UI.ViewModels;
using DevExpress.Mvvm;
using Moonlight;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Chortke.Models
{
	public class DocumentModel : ModelBase, ITrackable
	{
		public DateTime CreateDate { get => GetProperty(() => CreateDate); set => SetProperty(() => CreateDate, value); }
		public string? Description { get; set; }

		public long Debit => Articles.Where(a => a.IsDebit).Sum(a => a.Amount);
		public long Credit => Articles.Where(a => a.IsCredit).Sum(a => a.Amount);

		public void RaiseDebitCreditChanged() => RaisePropertiesChanged(nameof(Debit), nameof(Credit));

		[Ignore]
		public ObservableCollection<ArticleModel> Articles { get; set; } = default!;

		public ICommand EditCommand => new DelegateCommand(() => MyModuleManager.InjectTopMost(Modules.Document, this));
		public ICommand DeleteCommand => new DelegateCommand(() =>
		{
			DBHandler.Delete(this.Map<Document>());
			if (MyModuleManager.DefaultManager.IsInjected(Regions.Main, Modules.Documents))
				Messenger.Default.Send(DocumentsViewModel.Action.Update);
		});

		public DocumentModel()
		{
			Articles = [];
		}
	}
}
