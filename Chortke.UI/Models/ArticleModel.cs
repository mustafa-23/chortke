using Chortke.Data;
using Chortke.UI.Common;
using DevExpress.Mvvm;
using Moonlight;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Chortke.Models
{
	public partial class ArticleModel : ModelBase, ITrackable
	{
		public int BookId { get; set; }
		public DateTime CreateDate { get => GetProperty(() => CreateDate); set => SetProperty(() => CreateDate, value); }
		public long Amount { get => GetProperty(() => Amount); set => SetProperty(() => Amount, value); }
		public bool IsDebit { get => GetProperty(() => IsDebit); set => SetProperty(() => IsDebit, value, () => RaisePropertyChanged(nameof(IsCredit))); }
		public string? Description { get => GetProperty(() => Description); set => SetProperty(() => Description, value, () => RaisePropertyChanged(nameof(IsCredit))); }
		public string? Tag { get => GetProperty(() => Tag); set => SetProperty(() => Tag, value); }

		public bool IsCredit => !IsDebit;
		[Ignore]
		public BookModel Book { get => GetProperty(() => Book); set => SetProperty(() => Book, value, () => BookId = Book?.Id ?? 0); }
		[Ignore]
		public DocumentModel Document { get; set; } = default!;

		#region Commands
		public ICommand DeleteCommand => new DelegateCommand(Delete);
		private void Delete()
		{
			var result = 1;
			if (Id > 0)
				result = DBHandler.Delete(this.Map<Article>());
			if (result > 0 && Document != null)
				Document.Articles.Remove(this);
		}
		public ICommand ExtractAmountFromDescCommand => new DelegateCommand(ExtractAmountFromDesc, () => !string.IsNullOrEmpty(Description) && NumberReg().IsMatch(Description));
		private void ExtractAmountFromDesc()
		{
			var numbers = NumberReg().Matches(Description);
			if (numbers.Count > 0)
			{
				var values = numbers.Select(x => int.Parse(x.Value.TrimStart('$')));
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
					Amount = values.Sum();
				else
					Amount = values.Where(x => x >= 1000).Sum() + values.Where(x => x < 1000).Sum(x => x * 1000);
			}
		}
		public ICommand AddTagCommand => new DelegateCommand(AddTag);
		private void AddTag()
		{
			if (string.IsNullOrEmpty(Description))
				return;

			var index = Description.LastIndexOf(' ');
			if (index != -1)
			{
				var tag = Description[index..].Trim();
				Description = Description[..index];
				Tag += $"{tag};";
			}
			else
			{
				Tag += $"{Description};";
				Description = string.Empty;
			}
		}
		#endregion

		[GeneratedRegex(@"\$[\d]+")]
		private static partial Regex NumberReg();
	}
}